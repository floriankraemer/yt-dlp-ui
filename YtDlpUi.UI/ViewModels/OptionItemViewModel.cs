using YtDlpUi.Core.Models;

namespace YtDlpUi.UI.ViewModels;

public sealed class OptionItemViewModel : ViewModelBase
{
    private object? _value;

    public OptionItemViewModel(YtDlpOptionDefinition definition, object? initialValue)
    {
        Definition = definition;
        _value = initialValue ?? ParseDefault(definition);
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
        set
        {
            if (string.Equals(ValueType, "int", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(value, out var intValue))
                Value = intValue;
            else if (string.Equals(ValueType, "double", StringComparison.OrdinalIgnoreCase)
                && double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                Value = doubleValue;
            else
                Value = value;
        }
    }

    private static object? ParseDefault(YtDlpOptionDefinition definition)
    {
        if (definition.DefaultValue is null)
            return definition.ValueType.Equals("bool", StringComparison.OrdinalIgnoreCase) ? false : string.Empty;

        if (definition.ValueType.Equals("bool", StringComparison.OrdinalIgnoreCase))
            return bool.TryParse(definition.DefaultValue, out var b) && b;

        if (definition.ValueType.Equals("int", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(definition.DefaultValue, out var i))
            return i;

        if (definition.ValueType.Equals("double", StringComparison.OrdinalIgnoreCase)
            && double.TryParse(definition.DefaultValue, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
            return d;

        return definition.DefaultValue;
    }
}
