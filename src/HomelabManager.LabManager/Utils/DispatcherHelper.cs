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
    public static void PostToUIThreadIfNecessary(Action toPost, DispatcherPriority priority)
    {
        if (toPost is null)
            throw new ArgumentNullException(nameof(toPost));

        if (!Program.IsInTestingMode && !Dispatcher.UIThread.CheckAccess())
            Dispatcher.UIThread.Post(toPost, priority);
        else
            toPost();
    }

    /// <summary>
    /// Invoke an operation asynchronously on the UI thread.
    /// </summary>
    public static Task InvokeAsync(Action toInvoke, DispatcherPriority priority)
    {
        if (toInvoke is null)
            throw new ArgumentNullException(nameof(toInvoke));

        if (!Program.IsInTestingMode)
        {
            return Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(toInvoke, priority));
        }
        else
        {
            toInvoke();
            return Task.CompletedTask;
        }
    }
}
