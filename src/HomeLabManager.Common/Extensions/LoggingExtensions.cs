using System.Runtime.CompilerServices;
using Serilog;

namespace HomeLabManager.Common.Extensions;

/// <summary>
/// Static class for logging extensions.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Return a logger including information about the calling method or property.
    /// </summary>
    public static ILogger ForCaller(this ILogger logger, [CallerMemberName] string caller = null) =>
        logger?.ForContext("MemberName", caller) ?? throw new ArgumentNullException(nameof(logger));
}
