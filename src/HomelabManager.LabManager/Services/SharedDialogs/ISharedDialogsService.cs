using Material.Dialog.Interfaces;
using Material.Dialog;

namespace HomeLabManager.Manager.Services.SharedDialogs
{
    /// <summary>
    /// Service interface for dialogs commonly used around the application.
    /// </summary>
    public interface ISharedDialogsService
    {
        /// <summary>
        /// Create and show a simple confirmation dialog.
        /// </summary>
        /// <param name="content">Content to display in the dialog or nothing if none is provided.</param>
        Task<bool> ShowSimpleYesNoDialog(string content = null);

        /// <summary>
        /// Show a simple saving in progress dialog.
        /// </summary>
        /// <param name="textLabel">Text to display in the dialog, will show nothing if none provided.</param>
        (IDialogWindow<DialogResult> Dialog, Task DialogTask) ShowSimpleSavingDataDialog(string textLabel = null);
    }
}
