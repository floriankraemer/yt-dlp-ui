using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class BinaryLocator : IBinaryLocator
{
    private readonly string _configRoot;

    public BinaryLocator(string configRoot) => _configRoot = configRoot;

    public string? ResolveYtDlpPath(AppConfiguration config)
    {
        var configured = ExecutablePathResolver.NormalizeIfExists(config.YtDlpPath);
        if (configured is not null)
            return configured;

        var bundled = GetBundledYtDlpPath();
        if (File.Exists(bundled))
            return bundled;

        return ExecutablePathResolver.FindOnPath(GetYtDlpExecutableName());
    }

    public string? ResolveFfmpegPath(AppConfiguration config)
    {
        var configured = ExecutablePathResolver.NormalizeIfExists(config.FfmpegPath);
        if (configured is not null)
            return configured;

        var bundled = GetBundledFfmpegPath();
        if (File.Exists(bundled))
            return bundled;

        return ExecutablePathResolver.FindOnPath(GetFfmpegExecutableName());
    }

    public string GetBundledYtDlpPath() =>
        Path.Combine(_configRoot, AppPaths.BinFolderName, "yt-dlp", GetYtDlpExecutableName());

    public string GetBundledFfmpegPath() =>
        Path.Combine(_configRoot, AppPaths.BinFolderName, "ffmpeg", GetFfmpegExecutableName());

    private static string GetYtDlpExecutableName() =>
        OperatingSystem.IsWindows() ? "yt-dlp.exe" : "yt-dlp";

    private static string GetFfmpegExecutableName() =>
        OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
}
