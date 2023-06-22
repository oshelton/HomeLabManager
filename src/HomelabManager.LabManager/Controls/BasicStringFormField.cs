using Avalonia;

namespace HomeLabManager.Manager.Controls;

/// <summary>
/// Simple string based form field class.
/// </summary>
public class BasicStringFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly StyledProperty<string> StringValueProperty =
        AvaloniaProperty.Register<FormField, string>(nameof(StringValue), null);

    /// <summary>
    /// Gets or sets a value for the Field's Label.
    /// </summary>
    public string StringValue
    {
        get => GetValue(StringValueProperty);
        set => SetValue(StringValueProperty, value);
    }
}
