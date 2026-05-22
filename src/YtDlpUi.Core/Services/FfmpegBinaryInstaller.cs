using YtDlpUi.Core.Abstractions;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class FfmpegBinaryInstaller : IBinaryInstaller
{
    private readonly IBinaryReleaseSource _releaseSource;
    private readonly IBinaryLocator _binaryLocator;
    private readonly BinaryDownloadHelper _downloadHelper;
    private readonly string _runtimeIdentifier;

    public FfmpegBinaryInstaller(
        IBinaryReleaseSource releaseSource,
        IBinaryLocator binaryLocator,
        BinaryDownloadHelper downloadHelper,
        string? runtimeIdentifier = null)
    {
        _releaseSource = releaseSource;
        _binaryLocator = binaryLocator;
        _downloadHelper = downloadHelper;
        _runtimeIdentifier = runtimeIdentifier ?? System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier;
    }

    public async Task<BinaryInstallResult> InstallAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = _releaseSource.GetFfmpegAsset(_runtimeIdentifier);
            var extractDir = Path.Combine(Path.GetDirectoryName(_binaryLocator.GetBundledFfmpegPath())!, "extract");
            var archivePath = Path.Combine(extractDir, asset.FileName);

            Directory.CreateDirectory(extractDir);
            await _downloadHelper.DownloadAsync(asset, archivePath, cancellationToken);

            var ffmpegPath = BinaryDownloadHelper.ExtractFfmpegFromArchive(archivePath, extractDir);
            var target = _binaryLocator.GetBundledFfmpegPath();
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);

            if (!string.Equals(Path.GetFullPath(ffmpegPath), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                File.Copy(ffmpegPath, target, overwrite: true);

            BinaryDownloadHelper.MakeExecutable(target);
            return BinaryInstallResult.Success(target);
        }
        catch (Exception ex)
        {
            var existing = _binaryLocator.GetBundledFfmpegPath();
            if (File.Exists(existing))
                return BinaryInstallResult.Success(existing);

            return BinaryInstallResult.Failure(ex.Message);
        }
    }
}
