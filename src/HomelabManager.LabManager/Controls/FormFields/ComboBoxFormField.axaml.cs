using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;

namespace HomeLabManager.Manager.Controls.FormFields;

/// <summary>
/// ComboBox based form field class.
/// </summary>
[TemplatePart(ComboBoxFormField.ComboBoxPartName, typeof(ComboBox))]
public partial class ComboBoxFormField : FormField
{
    /// Name for the ComboBox that backs the field.
    public const string ComboBoxPartName = "PART_ComboBox";

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (e is null)
            return;

        if (_currentComboBox is not null)
            _currentComboBox.PropertyChanged -= FieldComboBoxPropertyChanged;

        _currentComboBox = e.NameScope.Find<ComboBox>(ComboBoxPartName);
        _currentComboBox.ItemsSource = Items;
        _currentComboBox.SelectedItem = SelectedItem;
        _currentComboBox.PropertyChanged += FieldComboBoxPropertyChanged;
    }

    protected override void UpdateDataValidation(
            AvaloniaProperty property,
            BindingValueType state,
            Exception error)
    {
        if (property == ItemsProperty || property == SelectedItemProperty)
            DataValidationErrors.SetError(this, error);
    }

    private void FieldComboBoxPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == ComboBox.SelectedItemProperty)
            SelectedItem = _currentComboBox.SelectedItem;
    }

    private IEnumerable _items;
    private object _selectedItem;
    private ComboBox _currentComboBox;
}
