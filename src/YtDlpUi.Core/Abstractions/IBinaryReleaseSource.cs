using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IBinaryReleaseSource
{
    ReleaseAsset GetYtDlpAsset(string runtimeIdentifier);
    ReleaseAsset GetFfmpegAsset(string runtimeIdentifier);
    ReleaseAsset GetDenoAsset(string runtimeIdentifier);
}
