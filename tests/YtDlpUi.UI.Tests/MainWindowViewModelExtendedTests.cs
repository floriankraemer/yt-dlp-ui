using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class MainWindowViewModelExtendedTests
{
    [Fact]
    public async Task InstallYtDlpAsync_UpdatesConfigPath()
    {
        var queue = Substitute.For<IDownloadQueueService>();
        queue.Jobs.Returns(Array.Empty<DownloadJob>());

        var appConfig = Substitute.For<IAppConfigStore>();
        var config = new AppConfiguration();
        appConfig.LoadAsync(Arg.Any<CancellationToken>()).Returns(config);
        appConfig.SaveAsync(Arg.Any<AppConfiguration>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                config = ci.Arg<AppConfiguration>();
                return Task.CompletedTask;
            });

        var installer = Substitute.For<IBinaryInstaller>();
        installer.InstallAsync(Arg.Any<CancellationToken>())
            .Returns(BinaryInstallResult.Success("/tmp/yt-dlp"));

        var vm = new MainWindowViewModel(
            queue,
            appConfig,
            Substitute.For<IProfileStore>(),
            ViewModelTestHelpers.CreateEnqueueCoordinator(queue, appConfig),
            new DownloadFolderService(),
            new YouTubeUrlNormalizer(),
            installer,
            Substitute.For<IBinaryInstaller>(),
            new BinaryLocator(Path.GetTempPath()));

        await vm.InstallYtDlpAsync();
        await appConfig.Received().SaveAsync(Arg.Is<AppConfiguration>(c => c.YtDlpPath == "/tmp/yt-dlp"), Arg.Any<CancellationToken>());
    }
}
