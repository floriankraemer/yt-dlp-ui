using Avalonia.Controls;

namespace YtDlpUi.UI.Services;

public static class QueueColumnLayout
{
    public static IReadOnlyDictionary<string, double> DefaultWidths { get; } =
        new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["status"] = 90,
            ["title"] = 220,
            ["url"] = 280,
            ["progress"] = 140,
            ["speed"] = 80,
            ["eta"] = 80,
            ["error"] = 220,
            ["actions"] = 260,
        };

    public static string? GetColumnKey(DataGridColumn column) =>
        column.Header?.ToString() switch
        {
            "Status" => "status",
            "Title" => "title",
            "URL" => "url",
            "Progress" => "progress",
            "Speed" => "speed",
            "ETA" => "eta",
            "Error" => "error",
            "Actions" => "actions",
            _ => null,
        };

    public static void Apply(DataGrid grid, IReadOnlyDictionary<string, double>? savedWidths)
    {
        var widths = savedWidths is { Count: > 0 } ? savedWidths : DefaultWidths;

        foreach (var column in grid.Columns)
        {
            var key = GetColumnKey(column);
            if (key is null || !widths.TryGetValue(key, out var pixels) || pixels < 40)
                continue;

            column.Width = new DataGridLength(pixels, DataGridLengthUnitType.Pixel);
        }
    }

    public static Dictionary<string, double> Capture(DataGrid grid)
    {
        var result = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var column in grid.Columns)
        {
            var key = GetColumnKey(column);
            if (key is null)
                continue;

            var pixels = column.ActualWidth > 0
                ? column.ActualWidth
                : column.Width.IsAbsolute
                    ? column.Width.DisplayValue
                    : 0;

            if (pixels >= 40)
                result[key] = Math.Round(pixels, 1);
        }

        return result;
    }
}
