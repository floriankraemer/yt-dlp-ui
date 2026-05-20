namespace YtDlpUi.Core.Constants;

public static class BinaryReleaseManifest
{
    public const string YtDlpReleaseTag = "2024.12.23";
    public const string FfmpegBuildId = "2024-12-23-git";

    public static string GetYtDlpAssetName(string runtimeIdentifier) =>
        runtimeIdentifier.StartsWith("win", StringComparison.OrdinalIgnoreCase)
            ? "yt-dlp.exe"
            : "yt-dlp";

    public static string GetYtDlpDownloadUrl(string runtimeIdentifier) =>
        $"https://github.com/yt-dlp/yt-dlp/releases/download/{YtDlpReleaseTag}/{GetYtDlpAssetName(runtimeIdentifier)}";
}
