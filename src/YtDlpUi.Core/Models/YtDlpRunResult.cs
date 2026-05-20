namespace YtDlpUi.Core.Models;

public sealed class YtDlpRunResult
{
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
    public bool WasCancelled { get; init; }
}
