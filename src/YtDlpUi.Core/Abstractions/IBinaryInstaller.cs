using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IBinaryInstaller
{
    Task<BinaryInstallResult> InstallAsync(CancellationToken cancellationToken = default);
}
