namespace YtDlpUi.Core.Models;

public sealed class ReleaseAsset
{
    public required string DownloadUrl { get; init; }
    public required string FileName { get; init; }
    public required string ReleasePageUrl { get; init; }
    public string? Sha256 { get; init; }
    public long MinimumSizeBytes { get; init; }
}
