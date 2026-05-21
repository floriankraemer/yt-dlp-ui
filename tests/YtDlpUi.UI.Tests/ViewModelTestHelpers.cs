using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Services;

namespace YtDlpUi.UI.Tests;

internal static class ViewModelTestHelpers
{
    public static DownloadEnqueueCoordinator CreateEnqueueCoordinator(
        IDownloadQueueService? queue = null,
        IAppConfigStore? appConfig = null,
        IProfileStore? profileStore = null,
        BinaryLocator? binaryLocator = null) =>
        new(
            queue ?? Substitute.For<IDownloadQueueService>(),
            appConfig ?? Substitute.For<IAppConfigStore>(),
            profileStore ?? Substitute.For<IProfileStore>(),
            new YouTubeUrlNormalizer(),
            binaryLocator ?? new BinaryLocator(Path.GetTempPath()),
            new DownloadFolderService());
}
