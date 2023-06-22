using Avalonia;
using Avalonia.Controls;

namespace HomeLabManager.Manager.Controls;

/// <summary>
/// Base class for simple form fields.
/// </summary>
public abstract class FormField : UserControl
{
    /// <summary>
    /// Defines the <see cref="Label"/> property.
    /// </summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<FormField, string>(nameof(Label), "Label");

    /// <summary>
    /// Defines the <see cref="InfoTip"/> property.
    /// </summary>
    public static readonly StyledProperty<object> InfoTipProperty =
        AvaloniaProperty.Register<FormField, object>(nameof(InfoTip), null);

    /// <summary>
    /// Gets or sets a value for the Field's Label.
    /// </summary>
    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Gets or sets a value for the Field's InfoTip.
    /// </summary>
    public object InfoTip
    {
        get => GetValue(InfoTipProperty);
        set => SetValue(InfoTipProperty, value);
    }
}
