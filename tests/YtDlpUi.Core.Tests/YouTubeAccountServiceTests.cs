// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YouTubeAccountServiceTests : IDisposable
{
    private readonly string _root;
    private readonly YouTubeAccountService _service;

    public YouTubeAccountServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-youtube-account", Guid.NewGuid().ToString("N"));
        _service = new YouTubeAccountService(_root, new YtDlpProcessRunner());
    }

    [Fact]
    public async Task ImportAsync_CopiesFileAndSetsSignedInStatus()
    {
        var source = Path.Combine(_root, "source.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(source, "# Netscape HTTP Cookie File\n");

        var result = await _service.ImportAsync(source);

        Assert.True(result.Success);
        Assert.Equal(YouTubeAccountStatus.SignedIn, _service.GetStatus());
        Assert.NotNull(_service.ResolveCookiesPath());
        Assert.True(File.Exists(Path.Combine(_root, AppPaths.CookiesFolderName, AppPaths.YouTubeCookiesFileName)));
    }

    [Fact]
    public async Task SignOutAsync_RemovesCookiesFile()
    {
        var source = Path.Combine(_root, "source.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(source, "# Netscape HTTP Cookie File\n");
        await _service.ImportAsync(source);

        await _service.SignOutAsync();

        Assert.Equal(YouTubeAccountStatus.NotSignedIn, _service.GetStatus());
        Assert.Null(_service.ResolveCookiesPath());
    }

    [Fact]
    public async Task SignOutAsync_WhenNotSignedIn_IsNoOp()
    {
        await _service.SignOutAsync();

        Assert.Equal(YouTubeAccountStatus.NotSignedIn, _service.GetStatus());
    }

    [Fact]
    public async Task ImportAsync_MissingSource_ReturnsError()
    {
        var result = await _service.ImportAsync(Path.Combine(_root, "missing.txt"));

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task ImportAsync_InvalidHeader_ReturnsWarning()
    {
        var source = Path.Combine(_root, "source.txt");
        Directory.CreateDirectory(_root);
        File.WriteAllText(source, "not-a-netscape-file\n");

        var result = await _service.ImportAsync(source);

        Assert.True(result.Success);
        Assert.NotNull(result.Warning);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
