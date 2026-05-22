using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Constants;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class BinaryInstallService
{
    private readonly IAppConfigStore _appConfigStore;
    private readonly IBinaryLocator _binaryLocator;

    public BinaryInstallService(IAppConfigStore appConfigStore, IBinaryLocator binaryLocator)
    {
        _appConfigStore = appConfigStore;
        _binaryLocator = binaryLocator;
    }

    public async Task<BinaryInstallResult> InstallAsync(
        ManagedBinary binary,
        IBinaryInstaller installer,
        CancellationToken cancellationToken = default)
    {
        var result = await installer.InstallAsync(cancellationToken);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.InstalledPath))
            return result;

        var config = await _appConfigStore.LoadAsync(cancellationToken);
        switch (binary)
        {
            case ManagedBinary.YtDlp:
                config.YtDlpPath = result.InstalledPath;
                break;
            case ManagedBinary.Ffmpeg:
                config.FfmpegPath = result.InstalledPath;
                break;
            case ManagedBinary.Deno:
                config.JsRuntimeEngine = JsRuntimeEngines.Deno;
                config.JsRuntimePath = result.InstalledPath;
                break;
        }

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
