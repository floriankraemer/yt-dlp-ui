using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Abstractions;

public interface IYtDlpProcessRunner
{
    Task<YtDlpRunResult> RunAsync(
        YtDlpInvocation invocation,
        IProgress<string>? stdoutProgress = null,
        CancellationToken cancellationToken = default);
}
