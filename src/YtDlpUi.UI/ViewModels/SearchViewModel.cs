using System.Collections.ObjectModel;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.Services;

namespace YtDlpUi.UI.ViewModels;

public sealed class SearchViewModel : ViewModelBase
{
    private readonly IYtDlpSearchService _searchService;
    private readonly IAppConfigStore _appConfigStore;
    private readonly IProfileStore _profileStore;
    private readonly DownloadEnqueueCoordinator _enqueueCoordinator;
    private readonly IThumbnailLoader _thumbnailLoader;

    private string _searchQuery = string.Empty;
    private bool _isSearching;
    private string? _statusMessage;
    private string? _errorMessage;
    private CancellationTokenSource? _thumbnailLoadCts;

    public SearchViewModel(
        IYtDlpSearchService searchService,
        IAppConfigStore appConfigStore,
        IProfileStore profileStore,
        DownloadEnqueueCoordinator enqueueCoordinator,
        IThumbnailLoader thumbnailLoader)
    {
        _searchService = searchService;
        _appConfigStore = appConfigStore;
        _profileStore = profileStore;
        _enqueueCoordinator = enqueueCoordinator;
        _thumbnailLoader = thumbnailLoader;
        Results = new ObservableCollection<SearchResultViewModel>();
        Profiles = new ObservableCollection<DownloadProfile>();
    }

    public ObservableCollection<SearchResultViewModel> Results { get; }
    public ObservableCollection<DownloadProfile> Profiles { get; }

    public string SearchQuery
    {
        get => _searchQuery;
        set => SetProperty(ref _searchQuery, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        private set => SetProperty(ref _isSearching, value);
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

    public bool HasResults => Results.Count > 0;

    public async Task InitializeAsync()
    {
        await RefreshProfilesAsync();
    }

    public async Task RefreshProfilesAsync()
    {
        var profiles = (await _profileStore.ListAsync())
            .OrderBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Profiles.Clear();
        foreach (var profile in profiles)
            Profiles.Add(profile);
    }

    public async Task SearchAsync()
    {
        ErrorMessage = null;
        StatusMessage = null;
        CancelThumbnailLoads();

        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            ErrorMessage = "Enter a search query.";
            return;
        }

        IsSearching = true;
        Results.Clear();
        OnPropertyChanged(nameof(HasResults));

        try
        {
            var page = await _searchService.SearchAsync(SearchQuery);
            var config = await _appConfigStore.LoadAsync();
            var defaultProfile = Profiles.FirstOrDefault(p => p.Id == config.ActiveProfileId)
                ?? Profiles.FirstOrDefault();
            var profileList = Profiles.ToList();

            foreach (var result in page.Results)
            {
                var row = new SearchResultViewModel(
                    result,
                    profileList,
                    defaultProfile,
                    _enqueueCoordinator,
                    _thumbnailLoader);
                Results.Add(row);
            }

            OnPropertyChanged(nameof(HasResults));
            StatusMessage = Results.Count == 0
                ? "No videos found."
                : Results.Count == 1
                    ? "1 video found."
                    : $"{Results.Count} videos found.";

            _thumbnailLoadCts = new CancellationTokenSource();
            _ = LoadThumbnailsAsync(_thumbnailLoadCts.Token);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSearching = false;
        }
    }

    private async Task LoadThumbnailsAsync(CancellationToken cancellationToken)
    {
        foreach (var row in Results)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await row.LoadThumbnailAsync(cancellationToken);
        }
    }

    private void CancelThumbnailLoads()
    {
        _thumbnailLoadCts?.Cancel();
        _thumbnailLoadCts?.Dispose();
        _thumbnailLoadCts = null;
    }
}
