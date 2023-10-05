using Avalonia.Threading;

namespace HomeLabManager.Manager.Utils;

/// <summary>
/// Helper class for some Dispatcher operations.
/// </summary>
public static class DispatcherHelper
{
    /// <summary>
    /// Invoke the posted action on the UI Thread if exection is not currently on the UI thread.
    /// </summary>
    public static async Task PostToUIThreadIfNecessary(Action toInvoke, DispatcherPriority priority)
    {
        if (toInvoke is null)
            throw new ArgumentNullException(nameof(toInvoke));

        if (!Program.IsInTestingMode && !Dispatcher.UIThread.CheckAccess())
            await Dispatcher.UIThread.InvokeAsync(toInvoke, priority);
        else
            toInvoke();
    }

    /// <summary>
    /// Invoke an operation asynchronously on the UI thread.
    /// </summary>
    public static async Task InvokeAsync(Action toInvoke, DispatcherPriority priority)
    {
        if (toInvoke is null)
            throw new ArgumentNullException(nameof(toInvoke));

        if (!Program.IsInTestingMode)
            await Dispatcher.UIThread.InvokeAsync(toInvoke, priority);
        else
            toInvoke();
    }
}
