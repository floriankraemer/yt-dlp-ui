// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class SettingsCoordinatorTests : IDisposable
{
    private readonly string _root;
    private readonly SettingsCoordinator _coordinator;

    public SettingsCoordinatorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        var profiles = new ProfileStore(_root);
        var appConfig = new AppConfigStore(_root, profiles);
        var catalog = new YtDlpOptionCatalog();
        _coordinator = new SettingsCoordinator(
            appConfig,
            profiles,
            new YtDlpCommandBuilder(catalog, new ExtraArgsTokenizer()),
            new AppSettingsValidator(new ExtraArgsTokenizer(), new DownloadFolderService()),
            new BinaryLocator(_root),
            new JsRuntimeLocator(),
            new YouTubeAccountService(_root, new YtDlpProcessRunner()));
    }

    [Fact]
    public async Task TestFfmpegAsync_ReturnsNotFoundWhenMissing()
    {
        var result = await _coordinator.TestFfmpegAsync(new AppConfiguration());
        Assert.Contains("not found", result ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildCliPreview_IncludesCookiesWhenSignedIn()
    {
        var cookiesDir = Path.Combine(_root, YtDlpUi.Core.Constants.AppPaths.CookiesFolderName);
        Directory.CreateDirectory(cookiesDir);
        var cookiesPath = Path.Combine(cookiesDir, YtDlpUi.Core.Constants.AppPaths.YouTubeCookiesFileName);
        File.WriteAllText(cookiesPath, "# Netscape HTTP Cookie File\n");

        var config = new AppConfiguration();
        var profile = new DownloadProfile { Id = "p", Name = "P" };

        var preview = _coordinator.BuildCliPreview(config, profile);

        Assert.Contains("--cookies", preview, StringComparison.Ordinal);
        Assert.Contains(cookiesPath, preview, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildCliPreview_IncludesJsRuntimesWhenConfigured()
    {
        Directory.CreateDirectory(_root);
        var denoPath = Path.Combine(_root, "deno");
        File.WriteAllText(denoPath, string.Empty);

        var config = new AppConfiguration
        {
            JsRuntimeEngine = Core.Constants.JsRuntimeEngines.Deno,
            JsRuntimePath = denoPath,
        };
        var profile = new DownloadProfile { Id = "p", Name = "P" };

        var preview = _coordinator.BuildCliPreview(config, profile);
        Assert.Contains("--js-runtimes", preview, StringComparison.Ordinal);
        Assert.Contains("deno:", preview, StringComparison.Ordinal);
    }

    [Fact]
    public void ResolveYtDlpPath_UsesConfiguredPathWhenFileExists()
    {
        Directory.CreateDirectory(_root);
        var path = Path.Combine(_root, "custom-yt-dlp");
        File.WriteAllText(path, string.Empty);

        var config = new AppConfiguration { YtDlpPath = path };
        var resolved = _coordinator.ResolveYtDlpPath(config);

        Assert.Equal(Path.GetFullPath(path), resolved);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
