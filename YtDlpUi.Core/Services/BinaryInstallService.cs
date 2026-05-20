using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class BinaryInstallService
{
    private readonly IAppConfigStore _appConfigStore;
    private readonly BinaryLocator _binaryLocator;

    public BinaryInstallService(IAppConfigStore appConfigStore, BinaryLocator binaryLocator)
    {
        _appConfigStore = appConfigStore;
        _binaryLocator = binaryLocator;
    }

    public async Task<BinaryInstallResult> InstallAsync(
        string binaryName,
        IBinaryInstaller installer,
        CancellationToken cancellationToken = default)
    {
        var result = await installer.InstallAsync(cancellationToken);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.InstalledPath))
            return result;

        var config = await _appConfigStore.LoadAsync(cancellationToken);
        if (string.Equals(binaryName, "yt-dlp", StringComparison.OrdinalIgnoreCase))
            config.YtDlpPath = result.InstalledPath;
        else
            config.FfmpegPath = result.InstalledPath;

        await _appConfigStore.SaveAsync(config, cancellationToken);
        return BinaryInstallResult.Success(result.InstalledPath);
    }

    public async Task<(string YtDlpStatus, string FfmpegStatus)> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var config = await _appConfigStore.LoadAsync(cancellationToken);
        var ytDlp = _binaryLocator.ResolveYtDlpPath(config);
        var ffmpeg = _binaryLocator.ResolveFfmpegPath(config);
        return (
            ytDlp is null ? "yt-dlp: not found" : $"yt-dlp: {ytDlp}",
            ffmpeg is null ? "ffmpeg: not found" : $"ffmpeg: {ffmpeg}");
    }
}
