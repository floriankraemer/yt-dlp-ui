using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class MainWindowViewModelInstallTests
{
    [Fact]
    public async Task InstallFfmpegAsync_SavesFfmpegPath()
    {
        var queue = Substitute.For<IDownloadQueueService>();
        queue.Jobs.Returns(Array.Empty<DownloadJob>());

        var appConfig = Substitute.For<IAppConfigStore>();
        var config = new AppConfiguration();
        appConfig.LoadAsync(Arg.Any<CancellationToken>()).Returns(config);

        var ffmpegInstaller = Substitute.For<IBinaryInstaller>();
        ffmpegInstaller.InstallAsync(Arg.Any<CancellationToken>())
            .Returns(BinaryInstallResult.Success("/tmp/ffmpeg"));

        var vm = ViewModelTestHelpers.CreateMainViewModel(
            queue,
            appConfig,
            ffmpegInstaller: ffmpegInstaller,
            binaryInstallService: ViewModelTestHelpers.CreateBinaryInstallService(appConfig));

        await vm.InstallFfmpegAsync();
        Assert.Equal("/tmp/ffmpeg", config.FfmpegPath);
    }

    [Fact]
    public async Task InstallYtDlpAsync_SetsErrorOnFailure()
    {
        var queue = Substitute.For<IDownloadQueueService>();
        queue.Jobs.Returns(Array.Empty<DownloadJob>());

        var installer = Substitute.For<IBinaryInstaller>();
        installer.InstallAsync(Arg.Any<CancellationToken>())
            .Returns(BinaryInstallResult.Failure("network error"));

        var vm = ViewModelTestHelpers.CreateMainViewModel(
            queue,
            ytDlpInstaller: installer);

        await vm.InstallYtDlpAsync();
        Assert.Equal("network error", vm.ErrorMessage);
    }

    [Fact]
    public void JobsChanged_RefreshesCollection()
    {
        var job = new DownloadJob { Url = "https://example.com", ProfileId = "default" };
        var queue = Substitute.For<IDownloadQueueService>();
        queue.Jobs.Returns([job]);

        var vm = ViewModelTestHelpers.CreateMainViewModel(queue);

        queue.JobsChanged += Raise.EventWith(queue, EventArgs.Empty);
        Assert.Single(vm.Jobs);
    }
}
