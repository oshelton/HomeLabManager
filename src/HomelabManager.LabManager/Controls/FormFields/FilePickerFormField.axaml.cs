using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Controls.FormFields;
using HomeLabManager.Manager.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace HomeLabManager.Manager.Controls.FormFields;

/// <summary>
/// Form field class for a file picker.
/// </summary>
public partial class FilePickerFormField : FormField
{
    /// <summary>
    /// Defines the <see cref="FilePathProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FilePickerFormField, string> FilePathProperty =
        AvaloniaProperty.RegisterDirect<FilePickerFormField, string>(
            nameof(FilePath),
            o => o.FilePath,
            (o, v) => o.FilePath = v,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Defines the <see cref="FileTypeFiltersProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FilePickerFormField, IReadOnlyList<FilePickerFileType>> FileTypeFiltersProperty =
        AvaloniaProperty.RegisterDirect<FilePickerFormField, IReadOnlyList<FilePickerFileType>>(
            nameof(FileTypeFilters),
            o => o.FileTypeFilters,
            (o, v) => o.FileTypeFilters = v);

    /// <summary>
    /// Defines the <see cref="FileTypeFiltersProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FilePickerFormField, object> OpenFilePickerButtonToolTipProperty =
        AvaloniaProperty.RegisterDirect<FilePickerFormField, object>(
            nameof(OpenFilePickerButtonToolTip),
            o => o.OpenFilePickerButtonToolTip,
            (o, v) => o.OpenFilePickerButtonToolTip = v);

    /// <summary>
    /// Defines the <see cref="DialogTitleProperty"/> property.
    /// </summary>
    public static readonly DirectProperty<FilePickerFormField, string> DialogTitleProperty =
        AvaloniaProperty.RegisterDirect<FilePickerFormField, string>(
            nameof(DialogTitle),
            o => o.DialogTitle,
            (o, v) => o.DialogTitle = v);

    public FilePickerFormField()
    {
        InitializeComponent();

        if (!Avalonia.Controls.Design.IsDesignMode)
            _logManager = Program.ServiceProvider.Services.GetService<ILogManager>().CreateContextualizedLogManager<FilePickerFormField>();
    }

    /// <summary>
    /// Gets or sets the path to the file the field is for.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => SetAndRaise(FilePathProperty, ref _filePath, value);
    }

    /// <summary>
    /// Gets or sets the filters to be used to determine what files are allowed to be selected.
    /// </summary>
    public IReadOnlyList<FilePickerFileType> FileTypeFilters
    {
        get => _fileTypeFilters;
        set => SetAndRaise(FileTypeFiltersProperty, ref _fileTypeFilters, value);
    }

    /// <summary>
    /// Gets the tool tip to use for the button.
    /// </summary>
    public object OpenFilePickerButtonToolTip
    {
        get => _openFilePickerButtonToolTip;
        set => SetAndRaise(OpenFilePickerButtonToolTipProperty, ref _openFilePickerButtonToolTip, value);
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
        if (property == FilePathProperty)
            DataValidationErrors.SetError(this, error);
    }

    /// <summary>
    /// Open the file picker.
    /// </summary>
    private async void OnPickFileButtonClicked(object sender, RoutedEventArgs args)
    {
        var logger = _logManager.GetApplicationLogger();
        logger.Verbose("File picker named \"{Name}\" with title \"{Title}\" opened with filters \"{Filters}\"", Name, DialogTitle ?? Label, FileTypeFilters?.SelectMany(x => x.Patterns).Distinct());

        var openedFile = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            Title = DialogTitle ?? Label,
            FileTypeFilter = FileTypeFilters,
        }).ConfigureAwait(true);

        if (openedFile is not null && openedFile.Count == 1)
        {
            FilePath = openedFile[0].Path.LocalPath;
            logger.Information("File \"{FilePath}\" chosen for field named \"{Name}\"", FilePath, Name);
        }
    }

    private readonly ContextAwareLogManager<FilePickerFormField> _logManager;

    private string _filePath;
    private IReadOnlyList<FilePickerFileType> _fileTypeFilters;
    private object _openFilePickerButtonToolTip;
    private string _dialogTitle;
}
