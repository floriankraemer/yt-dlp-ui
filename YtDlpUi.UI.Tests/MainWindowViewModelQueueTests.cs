using NSubstitute;
using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;
using YtDlpUi.Core.Services;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class MainWindowViewModelQueueTests
{
    [Fact]
    public async Task CancelJobAsync_DelegatesToQueue()
    {
        var queue = Substitute.For<IDownloadQueueService>();
        var job = new DownloadJob { Url = "https://example.com", ProfileId = "default", Status = DownloadStatus.Running };
        queue.Jobs.Returns([job]);

        var vm = new MainWindowViewModel(
            queue,
            Substitute.For<IAppConfigStore>(),
            Substitute.For<IProfileStore>(),
            new DownloadFolderService(),
            new YouTubeUrlNormalizer(),
            Substitute.For<IBinaryInstaller>(),
            Substitute.For<IBinaryInstaller>(),
            new BinaryLocator(Path.GetTempPath()));

        await vm.CancelJobAsync(new DownloadJobViewModel(job));
        await queue.Received(1).CancelAsync(job.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncJobs_ReusesExistingViewModels()
    {
        var queue = Substitute.For<IDownloadQueueService>();
        var job = new DownloadJob { Url = "https://example.com", ProfileId = "default" };
        queue.Jobs.Returns([job]);

        var appConfig = Substitute.For<IAppConfigStore>();
        appConfig.EnsureBootstrapAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        appConfig.LoadAsync(Arg.Any<CancellationToken>()).Returns(new AppConfiguration());

        var vm = new MainWindowViewModel(
            queue,
            appConfig,
            Substitute.For<IProfileStore>(),
            new DownloadFolderService(),
            new YouTubeUrlNormalizer(),
            Substitute.For<IBinaryInstaller>(),
            Substitute.For<IBinaryInstaller>(),
            new BinaryLocator(Path.GetTempPath()));

        await vm.InitializeAsync();
        var first = vm.Jobs[0];
        queue.Jobs.Returns([job]);
        vm.GetType().GetMethod("SyncJobs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(vm, null);
        Assert.Same(first, vm.Jobs[0]);
    }
}
