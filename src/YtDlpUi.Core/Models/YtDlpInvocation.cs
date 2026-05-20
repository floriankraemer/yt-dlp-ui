namespace YtDlpUi.Core.Models;

public sealed class YtDlpInvocation
{
    public required string ExecutablePath { get; init; }
    public required IReadOnlyList<string> Arguments { get; init; }
    public string? WorkingDirectory { get; init; }
}
