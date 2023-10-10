using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using HomeLabManager.Manager.Controls.FormFields;

namespace HomeLabManager.Manager.Controls.FormFields;

/// <summary>
/// Simple string based form field class.
/// </summary>
public partial class BasicStringFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="StringValue"/> property.
    /// </summary>
    public static readonly DirectProperty<BasicStringFormField, string> StringValueProperty =
        AvaloniaProperty.RegisterDirect<BasicStringFormField, string>(
            nameof(StringValue),
            o => o.StringValue,
            (o, v) => o.StringValue = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            enableDataValidation: true);

    public BasicStringFormField() => InitializeComponent();

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
            DataValidationErrors.SetError(this, error);
    }

    private string _stringValue;
}
