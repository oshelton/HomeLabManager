using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace HomeLabManager.Manager.Controls;

/// <summary>
/// Simple string based form field class.
/// </summary>
public class BasicStringFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly DirectProperty<BasicStringFormField, string> StringValueProperty =
        AvaloniaProperty.RegisterDirect<BasicStringFormField, string>(
            nameof(StringValue),
            o => o.StringValue,
            (o, v) => o.StringValue = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            enableDataValidation: true);

    private string _stringValue;
    /// <summary>
    /// Gets or sets a value for the Field's value.
    /// </summary>
    public string StringValue
    {
        get => _stringValue;
        set => SetAndRaise(StringValueProperty, ref _stringValue, value);
    }

    protected override void UpdateDataValidation(
            AvaloniaProperty property,
            BindingValueType state,
            Exception error)
    {
        if (property == StringValueProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }
}
