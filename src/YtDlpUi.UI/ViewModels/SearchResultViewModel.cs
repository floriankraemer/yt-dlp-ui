// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using Avalonia.Media.Imaging;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.Services;

namespace YtDlpUi.UI.ViewModels;

public sealed class SearchResultViewModel : ViewModelBase
{
    private readonly DownloadEnqueueCoordinator _enqueueCoordinator;
    private readonly IThumbnailLoader _thumbnailLoader;
    private DownloadProfile? _selectedProfile;
    private Bitmap? _thumbnail;
    private bool _isLoadingThumbnail;
    private bool _isAdding;
    private string? _statusMessage;

    public SearchResultViewModel(
        YouTubeSearchResult result,
        IReadOnlyList<DownloadProfile> profiles,
        DownloadProfile? defaultProfile,
        DownloadEnqueueCoordinator enqueueCoordinator,
        IThumbnailLoader thumbnailLoader)
    {
        Result = result;
        Title = result.Title;
        Channel = result.Channel ?? string.Empty;
        WatchUrl = result.WatchUrl;
        Profiles = profiles;
        _enqueueCoordinator = enqueueCoordinator;
        _thumbnailLoader = thumbnailLoader;
        _selectedProfile = defaultProfile ?? profiles.FirstOrDefault();
    }

    public YouTubeSearchResult Result { get; }
    public string Title { get; }
    public string Channel { get; }
    public string WatchUrl { get; }
    public IReadOnlyList<DownloadProfile> Profiles { get; }

    public DownloadProfile? SelectedProfile
    {
        get => _selectedProfile;
        set => SetProperty(ref _selectedProfile, value);
    }

    public Bitmap? Thumbnail
    {
        get => _thumbnail;
        private set => SetProperty(ref _thumbnail, value);
    }

    public bool IsLoadingThumbnail
    {
        get => _isLoadingThumbnail;
        private set => SetProperty(ref _isLoadingThumbnail, value);
    }

    public bool IsAdding
    {
        get => _isAdding;
        private set => SetProperty(ref _isAdding, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool CanAdd => !IsAdding && SelectedProfile is not null;

    public async Task LoadThumbnailAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(Result.ThumbnailUrl))
            return;

        IsLoadingThumbnail = true;
        try
        {
            var bitmap = await _thumbnailLoader.LoadAsync(Result.ThumbnailUrl, cancellationToken);
            Thumbnail = bitmap;
        }
        finally
        {
            IsLoadingThumbnail = false;
            OnPropertyChanged(nameof(CanAdd));
        }
    }

    public async Task AddToQueueAsync()
    {
        if (SelectedProfile is null)
        {
            StatusMessage = "Select a profile.";
            return;
        }

        IsAdding = true;
        OnPropertyChanged(nameof(CanAdd));
        StatusMessage = null;

        try
        {
            var result = await _enqueueCoordinator.TryEnqueueAsync(WatchUrl, SelectedProfile.Id);
            StatusMessage = result.IsSuccess ? "Added to queue." : result.Error;
        }
        finally
        {
            IsAdding = false;
            OnPropertyChanged(nameof(CanAdd));
        }
    }
}
