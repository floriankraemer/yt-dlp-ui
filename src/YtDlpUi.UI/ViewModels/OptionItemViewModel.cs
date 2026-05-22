using YtDlpUi.Core.Models;

namespace YtDlpUi.UI.ViewModels;

public sealed class OptionItemViewModel : ViewModelBase
{
    private object? _value;

    public OptionItemViewModel(YtDlpOptionDefinition definition, object? initialValue)
    {
        Definition = definition;
        _value = initialValue ?? OptionValueParser.ParseDefault(definition);
    }

    public YtDlpOptionDefinition Definition { get; }
    public string Flag => Definition.Flag;
    public string Tooltip => Definition.Tooltip;
    public string ValueType => Definition.ValueType;
    public IReadOnlyList<string> Choices => Definition.Choices;
    public bool IsBool => string.Equals(ValueType, "bool", StringComparison.OrdinalIgnoreCase);
    public bool IsChoice => string.Equals(ValueType, "choice", StringComparison.OrdinalIgnoreCase);
    public bool IsStringOption => !IsBool && !IsChoice;

    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public bool BoolValue
    {
        get => _value is true;
        set => Value = value;
    }

    public string StringValue
    {
        get => Convert.ToString(_value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        set => Value = OptionValueParser.ParseFromString(ValueType, value);
    }
}
