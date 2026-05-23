// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class YtDlpSearchServiceTests : IDisposable
{
    private readonly string _root;

    public YtDlpSearchServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-search-tests", Guid.NewGuid().ToString("N"));
        var ytDlpDir = Path.Combine(_root, "bin", "yt-dlp");
        Directory.CreateDirectory(ytDlpDir);
        File.WriteAllText(Path.Combine(ytDlpDir, "yt-dlp"), string.Empty);
    }

    [Fact]
    public async Task SearchAsync_ParsesRunnerOutput()
    {
        const string json = """
            {
              "entries": [
                {
                  "id": "vid1",
                  "title": "Test Video",
                  "channel": "Test Channel",
                  "webpage_url": "https://www.youtube.com/watch?v=vid1"
                }
              ]
            }
            """;

        var profiles = new ProfileStore(_root);
        var appConfig = new AppConfigStore(_root, profiles);
        await appConfig.EnsureBootstrapAsync();
        var config = await appConfig.LoadAsync();
        config.YtDlpPath = Path.Combine(_root, "bin", "yt-dlp", "yt-dlp");
        await appConfig.SaveAsync(config);

        var service = new YtDlpSearchService(
            new SearchRunner(json),
            appConfig,
            new BinaryLocator(_root),
            new JsRuntimeLocator(),
            new YouTubeAccountService(_root, new YtDlpProcessRunner()));

        var page = await service.SearchAsync("test query");

        Assert.Equal("test query", page.Query);
        Assert.Single(page.Results);
        Assert.Equal("Test Video", page.Results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_Throws()
    {
        var service = new YtDlpSearchService(
            new SearchRunner("{}"),
            new AppConfigStore(_root, new ProfileStore(_root)),
            new BinaryLocator(_root),
            new JsRuntimeLocator(),
            new YouTubeAccountService(_root, new YtDlpProcessRunner()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SearchAsync("   "));
    }

    [Fact]
    public void BuildSearchArguments_FirstPage_UsesPlaylistRange()
    {
        var args = YtDlpSearchService.BuildSearchArguments(
            new AppConfiguration(),
            new JsRuntimeLocator(),
            "hello world");

        Assert.Contains("--playlist-start", args);
        Assert.Contains("1", args);
        Assert.Contains("--playlist-end", args);
        Assert.Contains("20", args);
        Assert.Equal($"ytsearch{YtDlpSearchService.PageSize}:hello world", args[^1]);
    }

    [Fact]
    public void BuildSearchArguments_IncludesCookiesWhenProvided()
    {
        var cookiesPath = Path.Combine(_root, "youtube.txt");
        File.WriteAllText(cookiesPath, "# Netscape HTTP Cookie File\n");

        var args = YtDlpSearchService.BuildSearchArguments(
            new AppConfiguration(),
            new JsRuntimeLocator(),
            cookiesPath,
            "hello");

        Assert.Contains("--cookies", args);
        Assert.Contains(cookiesPath, args);
    }

    [Fact]
    public void BuildSearchArguments_SecondPage_OffsetsPlaylistRange()
    {
        var args = YtDlpSearchService.BuildSearchArguments(
            new AppConfiguration(),
            new JsRuntimeLocator(),
            "hello world",
            skip: 20);

        Assert.Contains("--playlist-start", args);
        Assert.Contains("21", args);
        Assert.Contains("--playlist-end", args);
        Assert.Contains("40", args);
        Assert.Equal("ytsearch40:hello world", args[^1]);
    }

    [Fact]
    public async Task SearchAsync_FullPage_SetsHasMoreResults()
    {
        var entryLines = Enumerable.Range(1, 20)
            .Select(i => $$"""{ "id": "vid{{i}}", "title": "Video {{i}}" }""");
        var json = $$"""
            { "entries": [ {{string.Join(", ", entryLines)}} ] }
            """;

        var profiles = new ProfileStore(_root);
        var appConfig = new AppConfigStore(_root, profiles);
        await appConfig.EnsureBootstrapAsync();
        var config = await appConfig.LoadAsync();
        config.YtDlpPath = Path.Combine(_root, "bin", "yt-dlp", "yt-dlp");
        await appConfig.SaveAsync(config);

        var service = new YtDlpSearchService(
            new SearchRunner(json),
            appConfig,
            new BinaryLocator(_root),
            new JsRuntimeLocator(),
            new YouTubeAccountService(_root, new YtDlpProcessRunner()));

        var page = await service.SearchAsync("query");

        Assert.Equal(20, page.Results.Count);
        Assert.True(page.HasMoreResults);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private sealed class SearchRunner(string output) : IYtDlpProcessRunner
    {
        public Task<YtDlpRunResult> RunAsync(
            YtDlpInvocation invocation,
            IProgress<string>? stdoutProgress = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new YtDlpRunResult
            {
                ExitCode = 0,
                StandardOutput = output,
            });
    }
}
