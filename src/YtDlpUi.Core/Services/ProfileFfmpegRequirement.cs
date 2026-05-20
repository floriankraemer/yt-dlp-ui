using System.Text.Json;
using YtDlpUi.Core.Models;

namespace YtDlpUi.Core.Services;

public static class ProfileFfmpegRequirement
{
    public const string FfmpegRequiredMessage =
        "ffmpeg is required for this profile (audio/video conversion). Install ffmpeg or set its path in Settings → Binaries.";

    public static bool RequiresFfmpeg(DownloadProfile profile)
    {
        if (ProfileHasTruthyOption(profile, "-x"))
            return true;

        if (ProfileHasNonEmptyStringOption(profile, "--audio-format"))
            return true;

        return ContainsToken(profile.ExtraArgs, "-x")
            || ContainsToken(profile.ExtraArgs, "--audio-format");
    }

    private static bool ProfileHasTruthyOption(DownloadProfile profile, string flag) =>
        profile.Options.TryGetValue(flag, out var value) && IsTruthy(value);

    private static bool ProfileHasNonEmptyStringOption(DownloadProfile profile, string flag) =>
        profile.Options.TryGetValue(flag, out var value)
        && !string.IsNullOrWhiteSpace(ReadString(value));

    private static bool IsTruthy(object? value) =>
        value switch
        {
            bool boolValue => boolValue,
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            string text => text.Equals("true", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };

    private static string? ReadString(object? value) =>
        value switch
        {
            string text => text,
            JsonElement { ValueKind: JsonValueKind.String } element => element.GetString(),
            _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
        };

    private static bool ContainsToken(string extraArgs, string token) =>
        !string.IsNullOrWhiteSpace(extraArgs)
        && extraArgs.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Any(part => string.Equals(part, token, StringComparison.Ordinal));
}
