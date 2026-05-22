using System.Collections.ObjectModel;
using Avalonia.Threading;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.Services;

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
    private readonly BinaryInstallService _binaryInstallService;
    private readonly DownloadFolderService _downloadFolderService;
    private readonly IFileSystemLauncher _fileSystemLauncher;

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
        BinaryInstallService binaryInstallService,
        IFileSystemLauncher fileSystemLauncher)
    {
        _queue = queue;
        _appConfigStore = appConfigStore;
        _profileStore = profileStore;
        _enqueueCoordinator = enqueueCoordinator;
        _downloadFolderService = downloadFolderService;
        _urlNormalizer = urlNormalizer;
        _ytDlpInstaller = ytDlpInstaller;
        _ffmpegInstaller = ffmpegInstaller;
        _binaryInstallService = binaryInstallService;
        _fileSystemLauncher = fileSystemLauncher;
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
            var (profiles, selected) = await ProfileListLoader.LoadWithSelectionAsync(_profileStore, _appConfigStore);
            Profiles.Clear();
            foreach (var profile in profiles)
                Profiles.Add(profile);

            _selectedProfile = selected;
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

        if (!_downloadFolderService.EnsureExists(normalized))
            return (false, "Could not create folder.");

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

    public bool TryOpenOutput(DownloadJobViewModel jobViewModel)
    {
        var job = _queue.Jobs.FirstOrDefault(item => item.Id == jobViewModel.Id);
        if (job is null)
        {
            ErrorMessage = "Download is no longer in the queue.";
            return false;
        }

        var target = DownloadOutputResolver.Resolve(job);
        if (!target.CanOpen)
        {
            ErrorMessage = "Could not find the downloaded file.";
            return false;
        }

        var opened = target.IsSingleFile
            ? _fileSystemLauncher.TryOpenFile(target.Path)
            : _fileSystemLauncher.TryOpenLocation(target.Path);

        if (!opened)
            ErrorMessage = "Could not open the download location.";

        return opened;
    }

    public async Task InstallYtDlpAsync() =>
        await RunInstallAsync(ManagedBinary.YtDlp, _ytDlpInstaller, "yt-dlp");

    public async Task InstallFfmpegAsync() =>
        await RunInstallAsync(ManagedBinary.Ffmpeg, _ffmpegInstaller, "ffmpeg");

    public async Task RefreshBinaryStatusAsync()
    {
        var (ytDlp, ffmpeg) = await _binaryInstallService.GetStatusAsync();
        YtDlpStatus = ytDlp;
        FfmpegStatus = ffmpeg;
    }

    private async Task RunInstallAsync(ManagedBinary binary, IBinaryInstaller installer, string displayName)
    {
        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = $"Installing {displayName}...";

        try
        {
            var result = await _binaryInstallService.InstallAsync(binary, installer);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error ?? $"Failed to install {displayName}.";
                return;
            }

            await RefreshBinaryStatusAsync();
            StatusMessage = $"{displayName} installed.";
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
        try
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                SyncJobs();
                return;
            }

            _ = Dispatcher.UIThread.InvokeAsync(SyncJobs);
        }
        catch (InvalidOperationException)
        {
            SyncJobs();
        }
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

        StatusMessage = QueueStatusMessageBuilder.Build(queueJobs);
    }
}
