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
            new JsRuntimeLocator());

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
            new JsRuntimeLocator());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SearchAsync("   "));
    }

    [Fact]
    public void BuildSearchArguments_IncludesYtsearchPrefix()
    {
        var args = YtDlpSearchService.BuildSearchArguments(
            new AppConfiguration(),
            new JsRuntimeLocator(),
            "hello world");

        Assert.Contains("--flat-playlist", args);
        Assert.Contains("--dump-single-json", args);
        Assert.Contains("--skip-download", args);
        Assert.Equal($"ytsearch{YtDlpSearchService.MaxResults}:hello world", args[^1]);
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
