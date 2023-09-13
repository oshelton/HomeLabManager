using System.Runtime.CompilerServices;
using HomeLabManager.Common.Utils;
using Serilog;

namespace HomeLabManager.Common.Services.Logging;

/// <summary>
/// Simple wrapper to allow easier type aware logging.
/// </summary>
public sealed class ContextAwareLogManager<T> where T : class
{
    /// <summary>
    /// Constructor, provides the ILogManager to be used.
    /// </summary>
    internal ContextAwareLogManager(ILogManager baseManager) => _baseManager = baseManager ?? throw new ArgumentNullException(nameof(baseManager));

    /// <summary>
    /// Get the application log with the pecified caller name provided.
    /// </summary>
    public ILogger GetApplicationLogger([CallerMemberName] string callerName = null) => _baseManager.GetApplicationLoggerForContext<T>(callerName);

    /// <summary>
    /// Start a new timed operation using the provided action and caller name.
    /// </summary>
    public DisposableOperation StartTimedOperation(string timedActionName, [CallerMemberName] string callerName = null) => _baseManager.StartTimedOperation<T>(timedActionName, callerName);

    private readonly ILogManager _baseManager;    
}
