using YtDlpUi.Core.Models;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class DownloadJobViewModelTests
{
    [Fact]
    public void Title_FallsBackToUrl()
    {
        var job = new DownloadJob { Url = "https://example.com", ProfileId = "default" };
        var vm = new DownloadJobViewModel(job);
        Assert.Equal(job.Url, vm.Title);
    }

    [Fact]
    public void CanOpenOutput_OnlyWhenCompletedWithExistingOutput()
    {
        var root = Path.Combine(Path.GetTempPath(), "yt-dlp-ui-vm-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var filePath = Path.Combine(root, "video.mp4");
        File.WriteAllText(filePath, "data");

        try
        {
            var job = new DownloadJob
            {
                Url = "https://example.com",
                ProfileId = "default",
                Status = DownloadStatus.Completed,
                WorkingDirectory = root,
            };
            job.OutputPaths.Add(filePath);

            var vm = new DownloadJobViewModel(job);
            Assert.True(vm.CanOpenOutput);
            Assert.Equal("Open file", vm.OpenOutputMenuLabel);

            job.OutputPaths.Add(Path.Combine(root, "other.mp4"));
            File.WriteAllText(job.OutputPaths[1], "b");
            vm.Refresh();
            Assert.Equal("Open Location", vm.OpenOutputMenuLabel);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void CanCancel_OnlyWhenQueuedOrRunning()
    {
        var job = new DownloadJob { Url = "https://example.com", ProfileId = "default", Status = DownloadStatus.Running };
        var vm = new DownloadJobViewModel(job);
        Assert.True(vm.CanCancel);
        job.Status = DownloadStatus.Completed;
        vm.Refresh();
        Assert.False(vm.CanCancel);
    }
}
