using System.IO.Compression;
using System.Net;
using System.Text;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class DenoBinaryInstallerTests : IDisposable
{
    private readonly string _root;

    public DenoBinaryInstallerTests() =>
        _root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task InstallAsync_DownloadsAndExtractsDeno()
    {
        var handler = new FakeZipHttpMessageHandler();
        var helper = new BinaryDownloadHelper(new HttpClient(handler));
        var source = new GitHubBinaryReleaseSource();
        var locator = new BinaryLocator(_root);
        var installer = new DenoBinaryInstaller(source, locator, helper, "linux-x64");

        var result = await installer.InstallAsync();

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(result.InstalledPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private sealed class FakeZipHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using var memory = new MemoryStream();
            using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry = archive.CreateEntry("deno");
                using var stream = entry.Open();
                var payload = Encoding.UTF8.GetBytes(new string('x', 1_000_100));
                stream.Write(payload, 0, payload.Length);
            }

            memory.Position = 0;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(memory.ToArray()),
            });
        }
    }
}
