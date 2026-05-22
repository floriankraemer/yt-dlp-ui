using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IJsRuntimeLocator
{
    string? ResolvePath(AppConfiguration config);
}
