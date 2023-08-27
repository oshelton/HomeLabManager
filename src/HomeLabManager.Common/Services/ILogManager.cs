using Serilog;

namespace HomeLabManager.Common.Services;

/// <summary>
/// Interface for retrieving application logging objects.
/// </summary>
public interface ILogManager
{
    /// <summary>
    /// Get the primary application logger.
    /// </summary>
    /// <returns>A Serilog logger to use to Log information.</returns>
    ILogger ApplicationLogger { get; }
}
