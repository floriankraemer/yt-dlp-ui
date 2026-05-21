using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
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

        var vm = new MainWindowViewModel(
            queue,
            appConfig,
            Substitute.For<IProfileStore>(),
            ViewModelTestHelpers.CreateEnqueueCoordinator(queue),
            new DownloadFolderService(),
            new YouTubeUrlNormalizer(),
            Substitute.For<IBinaryInstaller>(),
            ffmpegInstaller,
            new BinaryLocator(Path.GetTempPath()));

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

        var vm = new MainWindowViewModel(
            queue,
            Substitute.For<IAppConfigStore>(),
            Substitute.For<IProfileStore>(),
            ViewModelTestHelpers.CreateEnqueueCoordinator(queue),
            new DownloadFolderService(),
            new YouTubeUrlNormalizer(),
            installer,
            Substitute.For<IBinaryInstaller>(),
            new BinaryLocator(Path.GetTempPath()));

        await vm.InstallYtDlpAsync();
        Assert.Equal("network error", vm.ErrorMessage);
    }

    [Fact]
    public void JobsChanged_RefreshesCollection()
    {
        var job = new DownloadJob { Url = "https://example.com", ProfileId = "default" };
        var queue = Substitute.For<IDownloadQueueService>();
        queue.Jobs.Returns([job]);

        var vm = new MainWindowViewModel(
            queue,
            Substitute.For<IAppConfigStore>(),
            Substitute.For<IProfileStore>(),
            ViewModelTestHelpers.CreateEnqueueCoordinator(queue),
            new DownloadFolderService(),
            new YouTubeUrlNormalizer(),
            Substitute.For<IBinaryInstaller>(),
            Substitute.For<IBinaryInstaller>(),
            new BinaryLocator(Path.GetTempPath()));

        queue.JobsChanged += Raise.EventWith(queue, EventArgs.Empty);
        Assert.Single(vm.Jobs);
    }
}
