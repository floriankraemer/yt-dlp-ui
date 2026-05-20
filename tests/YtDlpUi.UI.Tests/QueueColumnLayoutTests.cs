using Avalonia.Controls;
using YtDlpUi.UI.Services;

namespace YtDlpUi.UI.Tests;

public sealed class QueueColumnLayoutTests
{
    [Fact]
    public void Capture_ReturnsWidthForEachKnownColumn()
    {
        var grid = CreateSampleGrid();
        QueueColumnLayout.Apply(grid, QueueColumnLayout.DefaultWidths);

        var captured = QueueColumnLayout.Capture(grid);

        Assert.Equal(8, captured.Count);
        Assert.Equal(90, captured["status"]);
        Assert.Equal(260, captured["actions"]);
    }

    [Fact]
    public void Apply_UsesSavedWidthsWhenProvided()
    {
        var grid = CreateSampleGrid();
        var saved = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["title"] = 400,
            ["url"] = 500,
        };

        QueueColumnLayout.Apply(grid, saved);

        var title = grid.Columns.First(c => QueueColumnLayout.GetColumnKey(c) == "title");
        Assert.Equal(400, title.Width.DisplayValue);
    }

    private static DataGrid CreateSampleGrid()
    {
        var grid = new DataGrid { AutoGenerateColumns = false };
        grid.Columns.Add(new DataGridTextColumn { Header = "Status", Width = Pixels(90) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Title", Width = Pixels(220) });
        grid.Columns.Add(new DataGridTextColumn { Header = "URL", Width = Pixels(280) });
        grid.Columns.Add(new DataGridTemplateColumn { Header = "Progress", Width = Pixels(140) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Speed", Width = Pixels(80) });
        grid.Columns.Add(new DataGridTextColumn { Header = "ETA", Width = Pixels(80) });
        grid.Columns.Add(new DataGridTextColumn { Header = "Error", Width = Pixels(220) });
        grid.Columns.Add(new DataGridTemplateColumn { Header = "Actions", Width = Pixels(200) });
        return grid;
    }

    private static DataGridLength Pixels(double value) =>
        new(value, DataGridLengthUnitType.Pixel);
}
