// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class ProfileStoreTests : IDisposable
{
    private readonly string _root;
    private readonly ProfileStore _store;

    public ProfileStoreTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        _store = new ProfileStore(_root);
    }

    [Fact]
    public async Task SaveAndGet_RoundTripsProfile()
    {
        var profile = new DownloadProfile
        {
            Id = "p1",
            Name = "Music",
            Options = new Dictionary<string, object?> { ["-f"] = "bestaudio" },
            ExtraArgs = "--simulate",
        };

        await _store.SaveAsync(profile);
        var loaded = await _store.GetAsync("p1");
        Assert.NotNull(loaded);
        Assert.Equal("Music", loaded.Name);
        Assert.Equal("--simulate", loaded.ExtraArgs);
    }

    [Fact]
    public async Task DeleteAsync_BlocksLastProfile()
    {
        await _store.SaveAsync(new DownloadProfile { Id = AppPaths.DefaultProfileId, Name = "Default" });
        await Assert.ThrowsAsync<InvalidOperationException>(() => _store.DeleteAsync(AppPaths.DefaultProfileId));
    }

    [Fact]
    public async Task DuplicateAsync_CreatesCopy()
    {
        await _store.SaveAsync(new DownloadProfile { Id = "src", Name = "Source", Options = { ["-f"] = "best" } });
        var copy = await _store.DuplicateAsync("src");
        Assert.NotEqual("src", copy.Id);
        Assert.Contains("(copy)", copy.Name);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
