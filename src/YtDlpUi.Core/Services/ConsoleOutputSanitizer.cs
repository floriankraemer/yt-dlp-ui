using System.Text.RegularExpressions;

namespace YtDlpUi.Core.Services;

public static class ConsoleOutputSanitizer
{
    private static readonly Regex AnsiEscapeRegex = new(
        @"\x1b\[[0-9;]*[A-Za-z]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string StripAnsi(string line) => AnsiEscapeRegex.Replace(line, string.Empty);
}
