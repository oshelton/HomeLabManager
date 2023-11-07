using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using HomeLabManager.Manager.Controls.FormFields;

namespace HomeLabManager.Manager.Controls.FormFields;

/// <summary>
/// ComboBox based form field class.
/// </summary>
public partial class ComboBoxFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="ItemsProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<ComboBoxFormField, IEnumerable> ItemsProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxFormField, IEnumerable>(
            nameof(Items),
            o => o.Items,
            (o, v) => o.Items = v,
            defaultBindingMode: Avalonia.Data.BindingMode.OneWay,
            enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="ItemsProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<ComboBoxFormField, object> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<ComboBoxFormField, object>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> ItemTemplateProperty =
        AvaloniaProperty.Register<ComboBoxFormField, IDataTemplate>(nameof(ItemTemplate));


    public ComboBoxFormField() => InitializeComponent();

    /// <summary>
    /// Gets or sets a value for the Field's Items.
    /// </summary>
    [Content]
    public IEnumerable Items
    {
        get => _items;
        set => SetAndRaise(ItemsProperty, ref _items, value);
    }

    /// <summary>
    /// Gets or sets a value for the Field's Selected Item.
    /// </summary>
    public object SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the content of the control.
    /// </summary>
    public IDataTemplate ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    protected override void UpdateDataValidation(
            AvaloniaProperty property,
            BindingValueType state,
            Exception error)
    {
        if (property == ItemsProperty || property == SelectedItemProperty)
            DataValidationErrors.SetError(this, error);
    }

    private IEnumerable _items;
    private object _selectedItem;
}
