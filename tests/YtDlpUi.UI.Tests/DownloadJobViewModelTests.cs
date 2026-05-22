// Copyright (C) 2026 Florian Krämer
// SPDX-License-Identifier: GPL-3.0-or-later
// See LICENSE for details.

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

    [Fact]
    public void DisplayProperties_ReflectJobState()
    {
        var job = new DownloadJob
        {
            Url = "https://example.com/watch?v=1",
            ProfileId = "default",
            Title = "My Video",
            Channel = "My Channel",
            Status = DownloadStatus.Failed,
            Progress = 42,
            Speed = "2MiB/s",
            Eta = "00:05",
            Error = "network error",
            LogOutput = "ERROR: failed",
        };

        var vm = new DownloadJobViewModel(job);

        Assert.Equal(job.Url, vm.Url);
        Assert.Equal("My Channel · My Video", vm.Title);
        Assert.Equal("Failed", vm.StatusText);
        Assert.Equal($"{job.Progress:0}%", vm.ProgressDisplayText);
        Assert.Equal("2MiB/s", vm.Speed);
        Assert.Equal("00:05", vm.Eta);
        Assert.Equal("network error", vm.Error);
        Assert.Equal("ERROR: failed", vm.LogOutput);
        Assert.True(vm.CanViewLog);
        Assert.True(vm.CanStart);
        Assert.True(vm.CanRemove);
        Assert.False(vm.CanCancel);
    }

    [Fact]
    public void CanStart_WhenQueued()
    {
        var job = new DownloadJob
        {
            Url = "https://example.com",
            ProfileId = "default",
            Status = DownloadStatus.Queued,
        };

        Assert.True(new DownloadJobViewModel(job).CanStart);
    }
}
