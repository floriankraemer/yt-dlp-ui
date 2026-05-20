using YtDlpUi.Core.Models;
using YtDlpUi.UI.ViewModels;

namespace YtDlpUi.UI.Tests;

public sealed class OptionItemViewModelExtendedTests
{
    [Fact]
    public void Constructor_ParsesBoolDefaultTrue()
    {
        var item = new OptionItemViewModel(
            new YtDlpOptionDefinition
            {
                Flag = "--simulate",
                Section = "Verbosity",
                ValueType = "bool",
                DefaultValue = "true",
                Tooltip = "Simulate",
            },
            null);

        Assert.True(item.BoolValue);
    }

    [Fact]
    public void StringValue_ParsesDoubleOption()
    {
        var item = new OptionItemViewModel(
            new YtDlpOptionDefinition
            {
                Flag = "--sleep-requests",
                Section = "Workarounds",
                ValueType = "double",
                Tooltip = "Sleep",
            },
            null);

        item.StringValue = "1.5";
        Assert.Equal(1.5, item.Value);
    }

    [Fact]
    public void IsStringOption_FalseForBoolAndChoice()
    {
        var boolItem = new OptionItemViewModel(
            new YtDlpOptionDefinition { Flag = "-q", Section = "Verbosity", ValueType = "bool", Tooltip = "q" },
            false);
        var choiceItem = new OptionItemViewModel(
            new YtDlpOptionDefinition
            {
                Flag = "--merge-output-format",
                Section = "Video Format",
                ValueType = "choice",
                Choices = ["mp4"],
                Tooltip = "fmt",
            },
            "mp4");

        Assert.False(boolItem.IsStringOption);
        Assert.False(choiceItem.IsStringOption);
    }
}
