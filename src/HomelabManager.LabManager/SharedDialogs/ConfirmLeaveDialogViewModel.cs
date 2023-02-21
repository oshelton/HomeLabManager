using DialogHostAvalonia;
using ReactiveUI;

namespace HomeLabManager.Manager.SharedDialogs;

internal sealed class ConfirmLeaveDialogViewModel : ReactiveObject
{
    public bool DoLeave { get; private set; }

    public void Continue()
    {
        DoLeave = true;
        DialogHost.Close(MainWindow.MainDialogHostId);
    }

    public void Cancel() => DialogHost.Close(MainWindow.MainDialogHostId);
}
