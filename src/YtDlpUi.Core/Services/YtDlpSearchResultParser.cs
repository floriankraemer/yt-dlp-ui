using System.Text.Json;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class YtDlpSearchResultParser
{
    public IReadOnlyList<YouTubeSearchResult> Parse(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return [];

        var trimmed = output.Trim();
        if (!trimmed.StartsWith('{'))
            return [];

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            var root = document.RootElement;

            if (root.TryGetProperty("entries", out var entries) && entries.ValueKind == JsonValueKind.Array)
                return ParseEntries(entries);

            if (root.TryGetProperty("id", out _))
                return ParseEntry(root) is { } single ? [single] : [];

            return [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<YouTubeSearchResult> ParseEntries(JsonElement entries)
    {
        var results = new List<YouTubeSearchResult>();
        foreach (var entry in entries.EnumerateArray())
        {
            var parsed = ParseEntry(entry);
            if (parsed is not null)
                results.Add(parsed);
        }

        return results;
    }

    private static YouTubeSearchResult? ParseEntry(JsonElement entry)
    {
        if (entry.ValueKind != JsonValueKind.Object)
            return null;

        var id = ReadString(entry, "id");
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var title = ReadString(entry, "title") ?? id;
        var channel = ReadChannel(entry);
        var watchUrl = ReadString(entry, "webpage_url")
            ?? $"https://www.youtube.com/watch?v={id}";
        var thumbnailUrl = ReadThumbnailUrl(entry, id);

        return new YouTubeSearchResult
        {
            VideoId = id,
            Title = title,
            Channel = channel,
            WatchUrl = watchUrl,
            ThumbnailUrl = thumbnailUrl,
        };
    }

    private static string? ReadChannel(JsonElement entry)
    {
        foreach (var propertyName in new[] { "channel", "uploader", "uploader_id" })
        {
            var value = ReadString(entry, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string? ReadThumbnailUrl(JsonElement entry, string videoId)
    {
        if (entry.TryGetProperty("thumbnail", out var thumbnail) && thumbnail.ValueKind == JsonValueKind.String)
        {
            var url = thumbnail.GetString();
            if (!string.IsNullOrWhiteSpace(url))
                return url;
        }

        if (entry.TryGetProperty("thumbnails", out var thumbnails))
        {
            var url = PickBestThumbnail(thumbnails);
            if (!string.IsNullOrWhiteSpace(url))
                return url;
        }

        return $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";
    }

    private static string? PickBestThumbnail(JsonElement thumbnails)
    {
        if (thumbnails.ValueKind == JsonValueKind.String)
            return thumbnails.GetString();

        if (thumbnails.ValueKind != JsonValueKind.Array)
            return null;

        string? best = null;
        var bestWidth = -1;

        foreach (var item in thumbnails.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var url = ReadString(item, "url");
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                continue;

            var width = item.TryGetProperty("width", out var widthElement) && widthElement.TryGetInt32(out var w)
                ? w
                : 0;

            if (width >= bestWidth)
            {
                bestWidth = width;
                best = url;
            }
        }

        return best;
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
