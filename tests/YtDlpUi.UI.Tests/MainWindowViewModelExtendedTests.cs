using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;

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

        var vm = ViewModelTestHelpers.CreateMainViewModel(
            queue,
            appConfig,
            ytDlpInstaller: installer,
            binaryInstallService: ViewModelTestHelpers.CreateBinaryInstallService(appConfig));

        await vm.InstallYtDlpAsync();
        await appConfig.Received().SaveAsync(Arg.Is<AppConfiguration>(c => c.YtDlpPath == "/tmp/yt-dlp"), Arg.Any<CancellationToken>());
    }
}
