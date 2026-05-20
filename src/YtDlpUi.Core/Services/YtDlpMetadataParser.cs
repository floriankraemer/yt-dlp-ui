using System.Text.Json;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class YtDlpMetadataParser
{
    public const string ChannelLinePrefix = "ytdlp-ui-channel:";
    public const string TitleLinePrefix = "ytdlp-ui-title:";
    public const string ChannelPrintTemplate = ChannelLinePrefix + "%(channel,uploader)s";
    public const string TitlePrintTemplate = TitleLinePrefix + "%(title)s";

    public static IReadOnlyList<string> BuildPrintArguments() =>
    [
        "--print", ChannelPrintTemplate,
        "--print", TitlePrintTemplate,
    ];

    public bool TryApplyMetadata(DownloadJob job, string line)
    {
        if (TryApplyPrintLine(job, line))
            return true;

        return TryApplyJsonLine(job, line);
    }

    public static string FormatQueueTitle(DownloadJob job)
    {
        var channel = NormalizeField(job.Channel);
        var title = NormalizeField(job.Title);

        if (channel is not null && title is not null)
            return $"{channel} · {title}";

        if (title is not null)
            return title;

        return job.Url;
    }

    private static bool TryApplyPrintLine(DownloadJob job, string line)
    {
        if (line.StartsWith(ChannelLinePrefix, StringComparison.Ordinal))
        {
            job.Channel = NormalizeField(line[ChannelLinePrefix.Length..]);
            return job.Channel is not null;
        }

        if (line.StartsWith(TitleLinePrefix, StringComparison.Ordinal))
        {
            job.Title = NormalizeField(line[TitleLinePrefix.Length..]);
            return job.Title is not null;
        }

        return false;
    }

    private static bool TryApplyJsonLine(DownloadJob job, string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith('{') || !trimmed.Contains("\"title\"", StringComparison.Ordinal))
            return false;

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            var root = document.RootElement;
            var changed = false;

            if (root.TryGetProperty("title", out var titleElement)
                && titleElement.ValueKind == JsonValueKind.String)
            {
                var title = NormalizeField(titleElement.GetString());
                if (title is not null && !string.Equals(job.Title, title, StringComparison.Ordinal))
                {
                    job.Title = title;
                    changed = true;
                }
            }

            var channel = ReadJsonChannel(root);
            if (channel is not null && !string.Equals(job.Channel, channel, StringComparison.Ordinal))
            {
                job.Channel = channel;
                changed = true;
            }

            return changed;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string? ReadJsonChannel(JsonElement root)
    {
        foreach (var propertyName in new[] { "channel", "uploader", "uploader_id" })
        {
            if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
                continue;

            var value = NormalizeField(element.GetString());
            if (value is not null)
                return value;
        }

        return null;
    }

    private static string? NormalizeField(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return string.Equals(trimmed, "NA", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }
}
