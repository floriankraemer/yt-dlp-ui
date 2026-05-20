using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;

namespace YtDlpUi.Core.Tests;

public sealed class BinaryInstallServiceTests
{
    [Fact]
    public async Task InstallAsync_UpdatesYtDlpPath()
    {
        var appConfig = Substitute.For<IAppConfigStore>();
        var config = new AppConfiguration();
        appConfig.LoadAsync(Arg.Any<CancellationToken>()).Returns(config);

        var installer = Substitute.For<IBinaryInstaller>();
        installer.InstallAsync(Arg.Any<CancellationToken>())
            .Returns(BinaryInstallResult.Success("/tmp/yt-dlp"));

        var service = new BinaryInstallService(appConfig, new BinaryLocator(Path.GetTempPath()));
        var result = await service.InstallAsync("yt-dlp", installer);
        Assert.True(result.IsSuccess);
        Assert.Equal("/tmp/yt-dlp", config.YtDlpPath);
    }

    [Fact]
    public async Task GetStatusAsync_ReportsMissingBinaries()
    {
        var appConfig = Substitute.For<IAppConfigStore>();
        appConfig.LoadAsync(Arg.Any<CancellationToken>()).Returns(new AppConfiguration());
        var service = new BinaryInstallService(appConfig, new BinaryLocator(Path.GetTempPath()));
        var (ytDlp, ffmpeg) = await service.GetStatusAsync();
        Assert.Contains("not found", ytDlp);
        Assert.Contains("not found", ffmpeg);
    }
}
