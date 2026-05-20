namespace YtDlpUi.Core.Models;

public sealed class YtDlpProgressParseResult
{
    public DownloadProgressPhase Phase { get; init; } = DownloadProgressPhase.Downloading;
    public double? ProgressPercent { get; init; }
    public string? Speed { get; init; }
    public string? Eta { get; init; }
    public string? ActivityLabel { get; init; }
    public bool UseIndeterminateProgress { get; init; }
}
