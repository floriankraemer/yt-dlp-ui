using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class GitHubBinaryReleaseSource : IBinaryReleaseSource
{
    public ReleaseAsset GetYtDlpAsset(string runtimeIdentifier) =>
        new()
        {
            DownloadUrl = BinaryReleaseManifest.GetYtDlpDownloadUrl(runtimeIdentifier),
            FileName = BinaryReleaseManifest.GetYtDlpAssetName(runtimeIdentifier),
            MinimumSizeBytes = 100_000,
        };

    public ReleaseAsset GetFfmpegAsset(string runtimeIdentifier)
    {
        var (fileName, url) = runtimeIdentifier switch
        {
            _ when runtimeIdentifier.Contains("win", StringComparison.OrdinalIgnoreCase) =>
                ("ffmpeg-master-latest-win64-gpl.zip", "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"),
            _ when runtimeIdentifier.Contains("osx", StringComparison.OrdinalIgnoreCase) =>
                ("ffmpeg-master-latest-macos64-gpl.zip", "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-macos64-gpl.zip"),
            _ =>
                ("ffmpeg-master-latest-linux64-gpl.tar.xz", "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-linux64-gpl.tar.xz"),
        };

        return new ReleaseAsset
        {
            DownloadUrl = url,
            FileName = fileName,
            MinimumSizeBytes = 1_000_000,
        };
    }
}
