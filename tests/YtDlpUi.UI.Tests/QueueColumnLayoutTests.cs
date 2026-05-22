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

        var title = grid.Columns.First(c => QueueColumnLayout.GetLayoutKey(c) == "title");
        Assert.Equal(400, title.Width.DisplayValue);
    }

    private static DataGrid CreateSampleGrid()
    {
        var grid = new DataGrid { AutoGenerateColumns = false };
        grid.Columns.Add(CreateColumn<DataGridTextColumn>("status", 90));
        grid.Columns.Add(CreateColumn<DataGridTextColumn>("title", 220));
        grid.Columns.Add(CreateColumn<DataGridTextColumn>("url", 280));
        grid.Columns.Add(CreateColumn<DataGridTemplateColumn>("progress", 140));
        grid.Columns.Add(CreateColumn<DataGridTextColumn>("speed", 80));
        grid.Columns.Add(CreateColumn<DataGridTextColumn>("eta", 80));
        grid.Columns.Add(CreateColumn<DataGridTextColumn>("error", 220));
        grid.Columns.Add(CreateColumn<DataGridTemplateColumn>("actions", 200));
        return grid;
    }

    private static T CreateColumn<T>(string key, double width)
        where T : DataGridColumn, new()
    {
        var column = new T { Width = Pixels(width) };
        QueueColumnLayout.SetLayoutKey(column, key);
        return column;
    }

    private static DataGridLength Pixels(double value) =>
        new(value, DataGridLengthUnitType.Pixel);
}
