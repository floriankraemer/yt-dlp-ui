using System.Collections.ObjectModel;
using Avalonia.Threading;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.UI.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IDownloadQueueService _queue;
    private readonly IAppConfigStore _appConfigStore;
    private readonly IProfileStore _profileStore;
    private readonly DownloadEnqueueCoordinator _enqueueCoordinator;
    private readonly YouTubeUrlNormalizer _urlNormalizer;
    private readonly IBinaryInstaller _ytDlpInstaller;
    private readonly IBinaryInstaller _ffmpegInstaller;
    private readonly BinaryLocator _binaryLocator;
    private readonly DownloadFolderService _downloadFolderService;

    private string _urlInput = string.Empty;
    private string? _statusMessage = "Paste a URL and click Add to download.";
    private string? _errorMessage;
    private string? _ytDlpStatus;
    private string? _ffmpegStatus;
    private bool _isBusy;
    private bool _loadingProfiles;
    private DownloadProfile? _selectedProfile;

    public MainWindowViewModel(
        IDownloadQueueService queue,
        IAppConfigStore appConfigStore,
        IProfileStore profileStore,
        DownloadEnqueueCoordinator enqueueCoordinator,
        DownloadFolderService downloadFolderService,
        YouTubeUrlNormalizer urlNormalizer,
        IBinaryInstaller ytDlpInstaller,
        IBinaryInstaller ffmpegInstaller,
        BinaryLocator binaryLocator)
    {
        _queue = queue;
        _appConfigStore = appConfigStore;
        _profileStore = profileStore;
        _enqueueCoordinator = enqueueCoordinator;
        _downloadFolderService = downloadFolderService;
        _urlNormalizer = urlNormalizer;
        _ytDlpInstaller = ytDlpInstaller;
        _ffmpegInstaller = ffmpegInstaller;
        _binaryLocator = binaryLocator;
        Jobs = new ObservableCollection<DownloadJobViewModel>();
        Profiles = new ObservableCollection<DownloadProfile>();
        _queue.JobsChanged += (_, _) => ScheduleRefreshJobs();
    }

    public ObservableCollection<DownloadJobViewModel> Jobs { get; }

    public ObservableCollection<DownloadProfile> Profiles { get; }

    public DownloadProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (!SetProperty(ref _selectedProfile, value) || _loadingProfiles || value is null)
                return;

            _ = PersistActiveProfileAsync(value.Id);
        }
    }

    public bool HasJobs => Jobs.Count > 0;

    public string UrlInput
    {
        get => _urlInput;
        set => SetProperty(ref _urlInput, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string? YtDlpStatus
    {
        get => _ytDlpStatus;
        private set => SetProperty(ref _ytDlpStatus, value);
    }

    public string? FfmpegStatus
    {
        get => _ffmpegStatus;
        private set => SetProperty(ref _ffmpegStatus, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public async Task InitializeAsync()
    {
        await _appConfigStore.EnsureBootstrapAsync();
        await RefreshProfilesAsync();
        await RefreshBinaryStatusAsync();
        SyncJobs();
    }

    public async Task RefreshProfilesAsync()
    {
        _loadingProfiles = true;
        try
        {
            var profiles = (await _profileStore.ListAsync())
                .OrderBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Profiles.Clear();
            foreach (var profile in profiles)
                Profiles.Add(profile);

            var config = await _appConfigStore.LoadAsync();
            _selectedProfile = Profiles.FirstOrDefault(profile => profile.Id == config.ActiveProfileId)
                ?? Profiles.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedProfile));
        }
        finally
        {
            _loadingProfiles = false;
        }
    }

    public async Task<bool> RequiresDownloadFolderSetupAsync()
    {
        var config = await _appConfigStore.LoadAsync();
        return !_downloadFolderService.IsConfigured(config);
    }

    public string GetSuggestedDownloadFolder() => DownloadFolderService.GetSuggestedDefaultPath();

    public void SetErrorMessage(string? message) => ErrorMessage = message;

    public async Task<(bool Saved, string? Error)> SaveDownloadFolderAsync(string path)
    {
        if (!_downloadFolderService.TryNormalize(path, out var normalized))
            return (false, "Enter a valid download folder path.");

        try
        {
            Directory.CreateDirectory(normalized);
        }
        catch (Exception ex)
        {
            return (false, $"Could not create folder: {ex.Message}");
        }

        var config = await _appConfigStore.LoadAsync();
        config.DownloadFolder = normalized;
        await _appConfigStore.SaveAsync(config);
        StatusMessage = $"Downloads will be saved to {normalized}";
        return (true, null);
    }

    public async Task<IReadOnlyDictionary<string, double>> LoadQueueColumnWidthsAsync()
    {
        var config = await _appConfigStore.LoadAsync();
        return config.QueueColumnWidths;
    }

    public async Task SaveQueueColumnWidthsAsync(IReadOnlyDictionary<string, double> widths)
    {
        var config = await _appConfigStore.LoadAsync();
        config.QueueColumnWidths = widths.ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal);
        await _appConfigStore.SaveAsync(config);
    }

    public async Task AddUrlAsync()
    {
        ErrorMessage = null;
        var config = await _appConfigStore.LoadAsync();
        var profileId = SelectedProfile?.Id ?? config.ActiveProfileId;
        var result = await _enqueueCoordinator.TryEnqueueAsync(UrlInput, profileId ?? string.Empty);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return;
        }

        UrlInput = string.Empty;
        SyncJobs();
        StatusMessage = Jobs.Count == 1
            ? "1 download in queue."
            : $"{Jobs.Count} downloads in queue.";
    }

    public void NormalizeUrlInput()
    {
        var result = _urlNormalizer.Normalize(UrlInput);
        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.NormalizedUrl))
            UrlInput = result.NormalizedUrl;
    }

    public async Task StartJobAsync(DownloadJobViewModel job)
    {
        ErrorMessage = null;
        await _queue.StartJobAsync(job.Id);
        SyncJobs();
        StatusMessage = "Download started.";
    }

    public async Task CancelJobAsync(DownloadJobViewModel job)
    {
        await _queue.CancelAsync(job.Id);
        SyncJobs();
    }

    public async Task RemoveJobAsync(DownloadJobViewModel job)
    {
        await _queue.RemoveAsync(job.Id);
        SyncJobs();
    }

    public async Task InstallYtDlpAsync()
    {
        await RunInstallAsync("yt-dlp", _ytDlpInstaller);
    }

    public async Task InstallFfmpegAsync()
    {
        await RunInstallAsync("ffmpeg", _ffmpegInstaller);
    }

    public async Task RefreshBinaryStatusAsync()
    {
        var installService = new BinaryInstallService(_appConfigStore, _binaryLocator);
        var (ytDlp, ffmpeg) = await installService.GetStatusAsync();
        YtDlpStatus = ytDlp;
        FfmpegStatus = ffmpeg;
    }

    private async Task RunInstallAsync(string name, IBinaryInstaller installer)
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = $"Installing {name}...";

        try
        {
            var installService = new BinaryInstallService(_appConfigStore, _binaryLocator);
            var result = await installService.InstallAsync(name, installer);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? $"Failed to install {name}.";
                return;
            }

            await RefreshBinaryStatusAsync();
            StatusMessage = $"{name} installed.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PersistActiveProfileAsync(string profileId)
    {
        var config = await _appConfigStore.LoadAsync();
        if (string.Equals(config.ActiveProfileId, profileId, StringComparison.Ordinal))
            return;

        config.ActiveProfileId = profileId;
        await _appConfigStore.SaveAsync(config);
    }

    private void ScheduleRefreshJobs()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            SyncJobs();
            return;
        }

        _ = Dispatcher.UIThread.InvokeAsync(SyncJobs);
    }

    private void SyncJobs()
    {
        var queueJobs = _queue.Jobs;
        var queueIds = queueJobs.Select(j => j.Id).ToHashSet();

        for (var i = Jobs.Count - 1; i >= 0; i--)
        {
            if (!queueIds.Contains(Jobs[i].Id))
                Jobs.RemoveAt(i);
        }

        foreach (var job in queueJobs)
        {
            var existing = Jobs.FirstOrDefault(j => j.Id == job.Id);
            if (existing is null)
                Jobs.Add(new DownloadJobViewModel(job));
            else
                existing.Refresh();
        }

        OnPropertyChanged(nameof(HasJobs));

        var failed = queueJobs.LastOrDefault(j => j.Status == DownloadStatus.Failed && !string.IsNullOrWhiteSpace(j.Error));
        if (failed is not null)
            ErrorMessage = failed.Error;

        var running = queueJobs.Count(j => j.Status == DownloadStatus.Running);
        var queued = queueJobs.Count(j => j.Status == DownloadStatus.Queued);
        var failedCount = queueJobs.Count(j => j.Status == DownloadStatus.Failed);

        if (running > 0)
            StatusMessage = running == 1 ? "Downloading…" : $"{running} downloads running…";
        else if (queued > 0)
            StatusMessage = queued == 1 ? "1 item queued (starting…)" : $"{queued} items queued (starting…)";
        else if (failedCount > 0)
            StatusMessage = failedCount == 1 ? "Download failed." : $"{failedCount} downloads failed.";
        else if (queueJobs.Count > 0 && queueJobs.All(j => j.Status == DownloadStatus.Completed))
            StatusMessage = queueJobs.Count == 1 ? "Download completed." : "All downloads completed.";
        else if (queueJobs.Count == 0)
            StatusMessage = "Paste a URL and click Add to download.";
    }
}
