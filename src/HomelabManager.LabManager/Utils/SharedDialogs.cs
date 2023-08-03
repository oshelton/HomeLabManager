using Avalonia.Controls;
using Material.Dialog;
using Material.Dialog.Interfaces;

namespace HomeLabManager.Manager.Utils;

/// <summary>
/// Classs for some simple and reusable dialogs.
/// </summary>
public static class SharedDialogs
{
    /// <summary>
    /// Create and show a simple confirmation dialog.
    /// </summary>
    /// <param name="content">Content to display in the dialog or nothing if none is provided.</param>
    public static async Task<bool> ShowSimpleYesNoDialog(string content = null)
    {
        var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
        {
            Borderless = true,
            ContentHeader = "Are you sure you want to continue?",
            SupportingText = content,
            DialogButtons = DialogHelper.CreateSimpleDialogButtons(Material.Dialog.Enums.DialogButtonsEnum.YesNo),
            DialogHeaderIcon = Material.Dialog.Icons.DialogIconKind.Warning
        });

        var result = await dialog.ShowDialog(MainWindow.Instance).ConfigureAwait(true);

        return result?.GetResult == "yes";
    }

    /// <summary>
    /// Show a simple saving in progress dialog.
    /// </summary>
    /// <param name="textLabel">Text to display in the dialog, will show nothing if none provided.</param>
    public static (IDialogWindow<DialogResult> Dialog, Task DialogTask) ShowSimpleSavingDataDialog(string textLabel = null)
    {
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
}
