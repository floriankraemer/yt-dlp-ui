using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

internal static class ViewModelTestHelpers
{
    public static AppSettingsValidator CreateValidator() =>
        new(new ExtraArgsTokenizer(), new DownloadFolderService());

    public static BinaryInstallService CreateBinaryInstallService(
        IAppConfigStore? appConfig = null,
        IBinaryLocator? binaryLocator = null) =>
        new(
            appConfig ?? Substitute.For<IAppConfigStore>(),
            binaryLocator ?? new BinaryLocator(Path.GetTempPath()));

    public static DownloadEnqueueCoordinator CreateEnqueueCoordinator(
        IDownloadQueueService? queue = null,
        IAppConfigStore? appConfig = null,
        IProfileStore? profileStore = null,
        IBinaryLocator? binaryLocator = null) =>
        new(
            queue ?? Substitute.For<IDownloadQueueService>(),
            appConfig ?? Substitute.For<IAppConfigStore>(),
            profileStore ?? Substitute.For<IProfileStore>(),
            new YouTubeUrlNormalizer(),
            binaryLocator ?? new BinaryLocator(Path.GetTempPath()),
            new DownloadFolderService());

    public static MainWindowViewModel CreateMainViewModel(
        IDownloadQueueService? queue = null,
        IAppConfigStore? appConfig = null,
        IProfileStore? profileStore = null,
        DownloadEnqueueCoordinator? enqueueCoordinator = null,
        DownloadFolderService? downloadFolderService = null,
        IBinaryInstaller? ytDlpInstaller = null,
        IBinaryInstaller? ffmpegInstaller = null,
        BinaryInstallService? binaryInstallService = null,
        IFileSystemLauncher? fileSystemLauncher = null)
    {
        if (queue is null)
        {
            queue = Substitute.For<IDownloadQueueService>();
            queue.Jobs.Returns(Array.Empty<DownloadJob>());
        }

        appConfig ??= Substitute.For<IAppConfigStore>();
        profileStore ??= Substitute.For<IProfileStore>();
        downloadFolderService ??= new DownloadFolderService();

        return new MainWindowViewModel(
            queue,
            appConfig,
            profileStore,
            enqueueCoordinator ?? CreateEnqueueCoordinator(queue, appConfig, profileStore),
            downloadFolderService,
            new YouTubeUrlNormalizer(),
            ytDlpInstaller ?? Substitute.For<IBinaryInstaller>(),
            ffmpegInstaller ?? Substitute.For<IBinaryInstaller>(),
            binaryInstallService ?? CreateBinaryInstallService(appConfig),
            fileSystemLauncher ?? Substitute.For<IFileSystemLauncher>());
    }
}
