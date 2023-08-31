using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HomeLabManager.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HomeLabManager.Manager.Controls;

/// <summary>
/// Form field class for a file picker.
/// </summary>
public partial class FolderPickerFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="FilePathProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FolderPickerFormField, string> FolderPathProperty =
        AvaloniaProperty.RegisterDirect<FolderPickerFormField, string>(
            nameof(FolderPath),
            o => o.FolderPath,
            (o, v) => o.FolderPath = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="OpenFolderPickerButtonToolTipProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FolderPickerFormField, object> OpenFolderPickerButtonToolTipProperty =
        AvaloniaProperty.RegisterDirect<FolderPickerFormField, object>(
            nameof(OpenFolderPickerButtonToolTip),
            o => o.OpenFolderPickerButtonToolTip,
            (o, v) => o.OpenFolderPickerButtonToolTip = v);

    /// <summary>
    /// Defines the <see cref="DialogTitleProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FolderPickerFormField, string> DialogTitleProperty =
        AvaloniaProperty.RegisterDirect<FolderPickerFormField, string>(
            nameof(DialogTitle),
            o => o.DialogTitle,
            (o, v) => o.DialogTitle = v);

    public FolderPickerFormField()
    {
        InitializeComponent();

        _logger = Program.ServiceProvider.Services.GetService<ILogManager>().ApplicationLogger.ForContext<FolderPickerFormField>();
    }

    /// <summary>
    /// Gets or sets the path to the file the field is for.
    /// </summary>
    public string FolderPath
    {
        get => _folderPath;
        set => SetAndRaise(FolderPathProperty, ref _folderPath, value);
    }

    /// <summary>
    /// Gets the tool tip to use for the button.
    /// </summary>
    public object OpenFolderPickerButtonToolTip
    {
        get => _openFolderPickerButtonToolTip;
        set => SetAndRaise(OpenFolderPickerButtonToolTipProperty, ref _openFolderPickerButtonToolTip, value);
    }

    /// <summary>
    /// Gets/sets the title of the opened file picker dialog.
    /// </summary>
    public string DialogTitle
    {
        get => _dialogTitle;
        set => SetAndRaise(DialogTitleProperty, ref _dialogTitle, value);
    }

    /// <summary>
    /// Handle propogating errors from the FilePathProperty
    /// </summary>
    protected override void UpdateDataValidation(
            AvaloniaProperty property,
            BindingValueType state,
            Exception error)
    {
        if (property == FolderPathProperty)
            DataValidationErrors.SetError(this, error);
    }

    /// <summary>
    /// Open the file picker.
    /// </summary>
    private async void OnPickFolderButtonClicked(object sender, RoutedEventArgs args)
    {
        _logger.ForCaller().Verbose("Folder picker named \"{Name}\" with title \"{Title}\" opened", Name, DialogTitle ?? Label);

        var chosenFolder = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = DialogTitle ?? Label,
        }).ConfigureAwait(true);

        if (chosenFolder is not null && chosenFolder.Count == 1)
        {
            FolderPath = chosenFolder[0].Path.LocalPath;
            _logger.ForCaller().Information("Folder \"{FolderPath}\" chosen for field named \"{Name}\"", FolderPath, Name);
        }
    }

    private readonly ILogger _logger;

    private string _folderPath;
    private object _openFolderPickerButtonToolTip;
    private string _dialogTitle;
}
