using Avalonia.Controls;
using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Windows;
using Material.Dialog;
using Material.Dialog.Interfaces;
using Serilog;

namespace HomeLabManager.Manager.Services.SharedDialogs;

/// <inheritdoc/>
public class SharedDialogsService : ISharedDialogsService
{
    /// <summary>
    /// Constructor, sets up a logger.
    /// </summary>
    public SharedDialogsService(ILogManager logManager) =>
        _logManager = logManager?.CreateContextualizedLogManager<SharedDialogsService>() ?? throw new ArgumentNullException(nameof(logManager));

    /// <inheritdoc/>
    public async Task<bool> ShowSimpleYesNoDialog(string content = null)
    {
        var logger = _logManager.GetApplicationLogger();

        logger.Information("ShowSimpleYesNoDialog Opened with content \"{Content}\"", content);
        var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
        {
            Borderless = true,
            ContentHeader = "Are you sure you want to continue?",
            SupportingText = content,
            DialogButtons = DialogHelper.CreateSimpleDialogButtons(Material.Dialog.Enums.DialogButtonsEnum.YesNo),
            DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Warning
        });

        var result = await dialog.ShowDialog(MainWindow.Instance).ConfigureAwait(true);

        logger.Information("ShowSimpleYesNoDialog Closed with result \"{Result}\"", result?.GetResult);
        return result?.GetResult == "yes";
    }

    /// <inheritdoc/>
    public (IDialogWindow<DialogResult> Dialog, Task DialogTask) ShowSimpleSavingDataDialog(string textLabel = null)
    {
        var logger = _logManager.GetApplicationLogger();
        logger.Information("Showing Simple saving dialog with label \"{Label}\"", textLabel);

        var layoutContainer = new StackPanel
        {
            MaxWidth = 350,
            Margin = new Avalonia.Thickness(8),
        };

        layoutContainer.Children.AddRange(new Control[]
        {
            new ProgressBar { IsIndeterminate = true },
            new TextBlock
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(16),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Text = textLabel,
            }
        });
        layoutContainer.Children[0].Classes.Add("circular");
        layoutContainer.Children[1].Classes.Add("Body1");

        var dialog = DialogHelper.CreateCustomDialog(new CustomDialogBuilderParams
        {
            ContentHeader = "Saving Data",
            Borderless = true,
            Content = layoutContainer,
            DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Info,
        });

        var showTask = dialog.ShowDialog(MainWindow.Instance);

        return (dialog, showTask);
    }

    private readonly ContextAwareLogManager<SharedDialogsService> _logManager;
}
