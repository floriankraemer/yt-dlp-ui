using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IBinaryLocator
{
    string? ResolveYtDlpPath(AppConfiguration config);

    string? ResolveFfmpegPath(AppConfiguration config);

    string GetBundledYtDlpPath();

    string GetBundledFfmpegPath();
}
