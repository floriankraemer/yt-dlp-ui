using YtDlpUi.Core.Models;

namespace YtDlpUi.UI.Services;

public static class QueueStatusMessageBuilder
{
    public static string Build(IReadOnlyList<DownloadJob> jobs)
    {
        if (jobs.Count == 0)
            return "Paste a URL and click Add to download.";

        var running = jobs.Count(j => j.Status == DownloadStatus.Running);
        var queued = jobs.Count(j => j.Status == DownloadStatus.Queued);
        var failedCount = jobs.Count(j => j.Status == DownloadStatus.Failed);

        if (running > 0)
            return running == 1 ? "Downloading…" : $"{running} downloads running…";

        if (queued > 0)
            return queued == 1 ? "1 item queued (starting…)" : $"{queued} items queued (starting…)";

        if (failedCount > 0)
            return failedCount == 1 ? "Download failed." : $"{failedCount} downloads failed.";

        if (jobs.All(j => j.Status == DownloadStatus.Completed))
            return jobs.Count == 1 ? "Download completed." : "All downloads completed.";

        return "Paste a URL and click Add to download.";
    }
}
