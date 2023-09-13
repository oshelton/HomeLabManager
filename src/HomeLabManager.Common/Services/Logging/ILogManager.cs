using System.Runtime.CompilerServices;
using HomeLabManager.Common.Utils;
using Serilog;

namespace HomeLabManager.Common.Services.Logging;

/// <summary>
/// Interface for retrieving application logging objects.
/// </summary>
public interface ILogManager
{
    /// <summary>
    /// Build a context aware log manager for the passed in type.
    /// </summary>
    ContextAwareLogManager<T> CreateContextualizedLogManager<T>() where T : class;

    /// <summary>
    /// Get the primary application logger.
    /// </summary>
    /// <returns>A Serilog logger to use to Log information.</returns>
    ILogger GetApplicationLoggerForContext<T>([CallerMemberName] string callerName = null);

    /// <summary>
    /// Begin a timed operation.
    /// </summary>
    /// <param name="timedActionName">Name of the action to be timed.</param>
    /// <returns>A DisposableOperation to be disposed to indicate the completion of the action.</returns>
    DisposableOperation StartTimedOperation<T>(string timedActionName, [CallerMemberName] string callerName = null);
}
