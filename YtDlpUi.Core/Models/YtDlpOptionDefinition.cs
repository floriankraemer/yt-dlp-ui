namespace YtDlpUi.Core.Models;

public sealed class YtDlpOptionDefinition
{
    public required string Flag { get; init; }
    public required string Section { get; init; }
    public required string ValueType { get; init; }
    public string? DefaultValue { get; init; }
    public required string Tooltip { get; init; }
    public IReadOnlyList<string> Choices { get; init; } = [];
}
