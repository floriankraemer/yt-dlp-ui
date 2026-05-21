namespace YtDlpUi.Core.Models;

public sealed class YouTubeSearchResult
{
    public required string VideoId { get; init; }
    public required string Title { get; init; }
    public string? Channel { get; init; }
    public required string WatchUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
}
