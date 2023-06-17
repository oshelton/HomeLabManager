using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace HomeLabManager.Manager.Utils;

public static class DispatcherHelper
{
    public static void PostToUIThread(Action toPost, DispatcherPriority priority)
    {
        if (toPost is null)
            throw new ArgumentNullException(nameof(toPost));

        if (!Program.IsInTestingMode)
            Dispatcher.UIThread.Post(toPost, priority);
        else
            toPost();
    }

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
