using System.Text.Json;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class DownloadQueueServiceTitleTests : IDisposable
{
    private readonly string _root;
    private readonly DownloadQueueService _queue;

    public DownloadQueueServiceTitleTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));
        var profiles = new ProfileStore(_root);
        var appConfig = new AppConfigStore(_root, profiles);
        var catalog = new YtDlpOptionCatalog();
        var ytDlpDir = Path.Combine(_root, "bin", "yt-dlp");
        Directory.CreateDirectory(ytDlpDir);
        File.WriteAllText(Path.Combine(ytDlpDir, "yt-dlp"), string.Empty);

        _queue = new DownloadQueueService(
            new TitleRunner(),
            new YtDlpCommandBuilder(catalog, new ExtraArgsTokenizer()),
            appConfig,
            profiles,
            new BinaryLocator(_root),
            new YouTubeUrlNormalizer(),
            new YtDlpProgressParser(),
            new YtDlpOutputPathParser(),
            new DownloadFolderService(),
            new JsRuntimeLocator());
    }

    [Fact]
    public async Task EnqueueAsync_ExtractsTitleFromOutput()
    {
        await new AppConfigStore(_root, new ProfileStore(_root)).EnsureBootstrapAsync();
        var config = await new AppConfigStore(_root, new ProfileStore(_root)).LoadAsync();
        config.YtDlpPath = Path.Combine(_root, "bin", "yt-dlp", "yt-dlp");
        config.DownloadFolder = _root;
        await new AppConfigStore(_root, new ProfileStore(_root)).SaveAsync(config);

        var job = await _queue.EnqueueAsync("https://www.youtube.com/watch?v=abc", config.ActiveProfileId);
        await WaitForCompletionAsync(job.Id);

        var completed = _queue.Jobs.Single(j => j.Id == job.Id);
        Assert.Equal("Sample title", completed.Title);
    }

    private async Task WaitForCompletionAsync(Guid jobId)
    {
        for (var i = 0; i < 100; i++)
        {
            var job = _queue.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job is { Status: DownloadStatus.Completed or DownloadStatus.Failed or DownloadStatus.Cancelled })
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private sealed class TitleRunner : IYtDlpProcessRunner
    {
        public Task<YtDlpRunResult> RunAsync(
            YtDlpInvocation invocation,
            IProgress<string>? stdoutProgress = null,
            CancellationToken cancellationToken = default)
        {
            stdoutProgress?.Report("{\"title\": \"Sample title\"}");
            stdoutProgress?.Report("download:PROGRESS=100%|SPEED=1MiB/s|ETA=00:00");
            return Task.FromResult(new YtDlpRunResult { ExitCode = 0 });
        }
    }
}
