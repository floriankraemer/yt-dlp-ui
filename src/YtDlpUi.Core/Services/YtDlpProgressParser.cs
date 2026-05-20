using System.Globalization;
using System.Text.RegularExpressions;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public sealed class YtDlpProgressParser
{
    public const string ProgressFieldsTemplate =
        "PROGRESS=%(progress._percent_str)s|SPEED=%(progress._speed_str)s|ETA=%(progress._eta_str)s";

    public const string DownloadProgressTemplate = "download:" + ProgressFieldsTemplate;
    public const string PostprocessProgressTemplate = "postprocess:" + ProgressFieldsTemplate;

    /// <summary>Alias for <see cref="DownloadProgressTemplate"/>.</summary>
    public const string ProgressTemplate = DownloadProgressTemplate;

    private static readonly string[] PostProcessMarkers =
    [
        "ExtractAudio",
        "Merger",
        "ffmpeg",
        "Metadata",
        "ThumbnailsConvertor",
        "ConvertThumbnail",
        "SponsorBlock",
        "ModifyChapters",
        "EmbedSubtitle",
        "EmbedThumbnail",
        "PostProcessor",
        "VideoConvertor",
        "Remux",
    ];

    private static readonly Regex AnsiEscapeRegex = new(
        @"\x1b\[[0-9;]*[A-Za-z]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex DefaultDownloadPercentRegex = new(
        @"\[download\][^\d%]*([\d.]+)\s*%",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex DefaultDownloadSpeedRegex = new(
        @"\bat\s+(\S+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex DefaultDownloadEtaRegex = new(
        @"\bETA\s+(\S+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PostProcessMarkerRegex = new(
        @"\[([A-Za-z][A-Za-z0-9]+)\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public bool TryParse(string line, out double progressPercent, out string? speed, out string? eta)
    {
        progressPercent = 0;
        speed = null;
        eta = null;

        if (!TryParse(line, downloadComplete: false, out var result))
            return false;

        progressPercent = result.ProgressPercent ?? 0;
        speed = result.Speed;
        eta = result.Eta;
        return result.ProgressPercent is not null
            || result.Speed is not null
            || result.Eta is not null
            || result.ActivityLabel is not null;
    }

    public bool TryParse(string line, bool downloadComplete, out YtDlpProgressParseResult result)
    {
        result = new YtDlpProgressParseResult();
        if (string.IsNullOrWhiteSpace(line))
            return false;

        line = StripAnsi(line).Trim();
        if (line.Length == 0)
            return false;

        if (TryParsePostProcessMarker(line, out var activityLabel))
        {
            result = new YtDlpProgressParseResult
            {
                Phase = DownloadProgressPhase.PostProcessing,
                ActivityLabel = activityLabel,
                UseIndeterminateProgress = true,
            };
            return true;
        }

        if (TryParseCustomTemplate(line, out var percent, out var speed, out var eta, out var hasPercent))
        {
            var phase = downloadComplete || percent >= 100
                ? DownloadProgressPhase.PostProcessing
                : DownloadProgressPhase.Downloading;

            result = new YtDlpProgressParseResult
            {
                Phase = phase,
                ProgressPercent = hasPercent ? percent : null,
                Speed = speed,
                Eta = eta,
                UseIndeterminateProgress = phase == DownloadProgressPhase.PostProcessing && !hasPercent,
            };
            return true;
        }

        if (TryParseDefaultDownloadLine(line, out percent, out speed, out eta))
        {
            var phase = downloadComplete || percent >= 100
                ? DownloadProgressPhase.PostProcessing
                : DownloadProgressPhase.Downloading;

            result = new YtDlpProgressParseResult
            {
                Phase = phase,
                ProgressPercent = percent,
                Speed = speed,
                Eta = eta,
                UseIndeterminateProgress = phase == DownloadProgressPhase.PostProcessing && percent >= 100,
            };
            return true;
        }

        return false;
    }

    private static bool TryParsePostProcessMarker(string line, out string? activityLabel)
    {
        activityLabel = null;
        if (line.Contains("[download]", StringComparison.OrdinalIgnoreCase))
            return false;

        foreach (Match match in PostProcessMarkerRegex.Matches(line))
        {
            var candidate = match.Groups[1].Value;
            if (PostProcessMarkers.Any(marker => string.Equals(marker, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                activityLabel = FormatActivityLabel(candidate);
                return true;
            }
        }

        return false;
    }

    private static string FormatActivityLabel(string raw) =>
        raw switch
        {
            "ExtractAudio" => "Converting audio",
            "Merger" => "Merging",
            "ffmpeg" => "Processing (ffmpeg)",
            "Metadata" => "Embedding metadata",
            "ThumbnailsConvertor" or "ConvertThumbnail" => "Converting thumbnail",
            "SponsorBlock" => "SponsorBlock",
            "ModifyChapters" => "Applying chapters",
            "EmbedSubtitle" => "Embedding subtitles",
            "EmbedThumbnail" => "Embedding thumbnail",
            "VideoConvertor" => "Converting video",
            "Remux" => "Remuxing",
            _ => raw,
        };

    private static bool TryParseCustomTemplate(
        string line,
        out double progressPercent,
        out string? speed,
        out string? eta,
        out bool hasPercent)
    {
        progressPercent = 0;
        speed = null;
        eta = null;
        hasPercent = false;

        if (!line.Contains("PROGRESS=", StringComparison.OrdinalIgnoreCase))
            return false;

        var recognizedTemplateLine = true;
        var parsed = false;
        foreach (var part in line.Split('|'))
        {
            var segment = part.Trim();
            var progressIndex = segment.IndexOf("PROGRESS=", StringComparison.OrdinalIgnoreCase);
            if (progressIndex >= 0)
            {
                var value = segment[(progressIndex + "PROGRESS=".Length)..];
                if (TryParsePercent(value, out var percent))
                {
                    progressPercent = percent;
                    hasPercent = true;
                    parsed = true;
                }
            }
            else if (segment.StartsWith("SPEED=", StringComparison.OrdinalIgnoreCase))
            {
                var value = segment["SPEED=".Length..].Trim();
                if (!IsUnavailable(value))
                {
                    speed = value;
                    parsed = true;
                }
            }
            else if (segment.StartsWith("ETA=", StringComparison.OrdinalIgnoreCase))
            {
                var value = segment["ETA=".Length..].Trim();
                if (!IsUnavailable(value))
                {
                    eta = value;
                    parsed = true;
                }
            }
        }

        return parsed || recognizedTemplateLine;
    }

    private static bool TryParseDefaultDownloadLine(
        string line,
        out double progressPercent,
        out string? speed,
        out string? eta)
    {
        progressPercent = 0;
        speed = null;
        eta = null;

        if (!line.Contains("[download]", StringComparison.OrdinalIgnoreCase))
            return false;

        var parsed = false;

        var percentMatch = DefaultDownloadPercentRegex.Match(line);
        if (percentMatch.Success && TryParsePercent(percentMatch.Groups[1].Value, out var percent))
        {
            progressPercent = percent;
            parsed = true;
        }

        var speedMatch = DefaultDownloadSpeedRegex.Match(line);
        if (speedMatch.Success && !IsUnavailable(speedMatch.Groups[1].Value))
        {
            speed = speedMatch.Groups[1].Value;
            parsed = true;
        }

        var etaMatch = DefaultDownloadEtaRegex.Match(line);
        if (etaMatch.Success && !IsUnavailable(etaMatch.Groups[1].Value))
        {
            eta = etaMatch.Groups[1].Value;
            parsed = true;
        }

        return parsed;
    }

    private static bool TryParsePercent(string raw, out double percent)
    {
        percent = 0;
        var value = raw.Trim().TrimEnd('%').Trim();
        if (IsUnavailable(value))
            return false;

        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out percent))
            return false;

        percent = Math.Clamp(percent, 0, 100);
        return true;
    }

    private static bool IsUnavailable(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || value.Equals("N/A", StringComparison.OrdinalIgnoreCase)
        || value.Equals("NA", StringComparison.OrdinalIgnoreCase);

    private static string StripAnsi(string line) => AnsiEscapeRegex.Replace(line, string.Empty);
}
