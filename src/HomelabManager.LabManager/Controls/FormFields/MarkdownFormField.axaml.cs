using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using HomeLabManager.Manager.Controls.FormFields;
using ReactiveUI;

namespace HomeLabManager.Manager.Controls.FormFields;

/// <summary>
/// Simple string based form field class.
/// </summary>
public partial class MarkdownFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="StringValue"/> property.
    /// </summary>
    public static readonly DirectProperty<MarkdownFormField, string> StringValueProperty =
        AvaloniaProperty.RegisterDirect<MarkdownFormField, string>(
            nameof(StringValue),
            o => o.StringValue,
            (o, v) => o.StringValue = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly DirectProperty<MarkdownFormField, string> RenderedTextProperty =
    AvaloniaProperty.RegisterDirect<MarkdownFormField, string>(
        nameof(RenderedText),
        o => o.RenderedText);

    public MarkdownFormField()
    {
        InitializeComponent();

        RenderedText = StringValue;

        this.WhenAnyValue(x => x.StringValue)
            .Throttle(TimeSpan.FromSeconds(1))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => RenderedText = x);
    }

    /// <summary>
    /// Gets or sets a value for the Field's value.
    /// </summary>
    public string StringValue
    {
        get => _stringValue;
        set => SetAndRaise(StringValueProperty, ref _stringValue, value);
    }

    /// <summary>
    /// Gets the text displayed in the preview.
    /// </summary>
    public string RenderedText
    {
        get => _renderedText;
        private set => SetAndRaise(RenderedTextProperty, ref _renderedText, value);
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
    private string _renderedText;
}
