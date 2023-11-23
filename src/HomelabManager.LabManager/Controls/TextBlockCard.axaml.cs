using System.Security;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Material.Icons;

namespace HomeLabManager.Manager.Controls;

/// <summary>
/// Reusable Loading In Progress Card.
/// </summary>
[TemplatePart(PartTextBlockName, typeof(TextBlock))]

public partial class TextBlockCard : TemplatedControl
{
    /// <summary>
    /// Name of the main TextBlock Template part.
    /// </summary>
    public const string PartTextBlockName = "PART_TextBlock";

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly DirectProperty<TextBlockCard, string> TextProperty =
        AvaloniaProperty.RegisterDirect<TextBlockCard, string>(
            nameof(Text), t => t.Text, (t, v) => t.Text= v);

    /// <summary>
    /// Defines the <see cref="Inlines"/> property.
    /// </summary>
    public static readonly DirectProperty<TextBlockCard, InlineCollection> InlinesProperty =
        AvaloniaProperty.RegisterDirect<TextBlockCard, InlineCollection>(
            nameof(Inlines), t => t.Inlines, (t, v) => t.Inlines = v);

    public TextBlockCard() => InitializeComponent();

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [Content]
    public string Text
    {
        get => _text;
        set => SetAndRaise(TextProperty, ref _text, value);
    }

    /// <summary>
    /// Gets or sets the inlines.
    /// </summary>
    [Content]
    public InlineCollection Inlines
    {
        get => _inlines;
        set => SetAndRaise(InlinesProperty, ref _inlines, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        base.OnApplyTemplate(e);

        _block = e.NameScope.Get<TextBlock>(PartTextBlockName) ?? throw new InvalidOperationException($"TextBlockCard templates must define a {PartTextBlockName} TextBlock element.");

        _block.Text = _text;
        _block.Inlines = Inlines;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        switch (change.Property.Name)
        {
            case nameof(Text):
                _block.Inlines = null;
                _block.Text = Text;
                break;
            case nameof(Inlines):
                _block.Text = null;
                _block.Inlines = Inlines;
                break;
        }
    }

    private string _text;
    private InlineCollection _inlines;

    private TextBlock _block;
}
