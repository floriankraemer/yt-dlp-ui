// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

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
    private bool _isLoadingMore;
    private bool _canLoadMore;
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

    public bool IsLoadingMore
    {
        get => _isLoadingMore;
        private set => SetProperty(ref _isLoadingMore, value);
    }

    public bool CanLoadMore
    {
        get => _canLoadMore;
        private set => SetProperty(ref _canLoadMore, value);
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
        var profiles = await ProfileListLoader.LoadOrderedAsync(_profileStore);
        Profiles.Clear();
        foreach (var profile in profiles)
            Profiles.Add(profile);
    }

    public Task SearchAsync() => FetchResultsAsync(reset: true);

    public Task LoadMoreAsync() => FetchResultsAsync(reset: false);

    private async Task FetchResultsAsync(bool reset)
    {
        if (reset)
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
            CanLoadMore = false;
            OnPropertyChanged(nameof(HasResults));
        }
        else
        {
            if (!CanLoadMore || IsSearching || IsLoadingMore)
                return;

            ErrorMessage = null;
            IsLoadingMore = true;
        }

        try
        {
            var skip = reset ? 0 : Results.Count;
            var page = await _searchService.SearchAsync(SearchQuery, skip);
            var config = await _appConfigStore.LoadAsync();
            var defaultProfile = Profiles.FirstOrDefault(p => p.Id == config.ActiveProfileId)
                ?? Profiles.FirstOrDefault();
            var profileList = Profiles.ToList();
            var existingIds = Results.Select(r => r.Result.VideoId).ToHashSet(StringComparer.Ordinal);

            var newRows = new List<SearchResultViewModel>();
            foreach (var result in page.Results)
            {
                if (!existingIds.Add(result.VideoId))
                    continue;

                var row = new SearchResultViewModel(
                    result,
                    profileList,
                    defaultProfile,
                    _enqueueCoordinator,
                    _thumbnailLoader);
                newRows.Add(row);
                Results.Add(row);
            }

            CanLoadMore = page.HasMoreResults && newRows.Count > 0;
            OnPropertyChanged(nameof(HasResults));
            UpdateStatusMessage();

            if (newRows.Count > 0)
            {
                if (reset)
                {
                    _thumbnailLoadCts?.Cancel();
                    _thumbnailLoadCts?.Dispose();
                    _thumbnailLoadCts = new CancellationTokenSource();
                }
                else
                {
                    _thumbnailLoadCts ??= new CancellationTokenSource();
                }

                _ = LoadThumbnailsAsync(newRows, _thumbnailLoadCts.Token);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            if (reset)
                IsSearching = false;
            else
                IsLoadingMore = false;
        }
    }

    private void UpdateStatusMessage()
    {
        if (Results.Count == 0)
        {
            StatusMessage = CanLoadMore ? null : "No videos found.";
            return;
        }

        StatusMessage = CanLoadMore
            ? Results.Count == 1 ? "1 loaded" : $"{Results.Count} loaded"
            : Results.Count == 1 ? "1 video found." : $"{Results.Count} videos found.";
    }

    private async Task LoadThumbnailsAsync(
        IReadOnlyList<SearchResultViewModel> rows,
        CancellationToken cancellationToken)
    {
        foreach (var row in rows)
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
