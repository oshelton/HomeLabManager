using System.Diagnostics;
using System.Globalization;
using Serilog;
using Serilog.Events;

namespace HomeLabManager.Common.Services;

/// <summary>
/// Main LogManager implementation.
/// </summary>
public sealed class LogManager : ILogManager
{
    public LogManager(bool isInTestingMode) => _isInTestingMode = isInTestingMode;

    /// <inheritdoc/>
    public ILogger ApplicationLogger 
    { 
        get 
        {
            if (_applicationLogger is null)
            {
                var loggerConfiguration = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Verbose();
                if (Debugger.IsAttached || _isInTestingMode)
                {
                    loggerConfiguration = loggerConfiguration.WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}.{MemberName}] {Message:lj}{NewLine}{Exception}");
                }
                else
                {
                    var nowStamp = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss", CultureInfo.InvariantCulture.DateTimeFormat);
                    loggerConfiguration = loggerConfiguration.WriteTo.File($@".\logs\log_{nowStamp}.txt",
                        outputTemplate: "[{Timestamp:MM-dd-yyyy_HH:mm:ss} {Level:u3}] [{SourceContext:l}.{MemberName}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information);
                }
                _applicationLogger = loggerConfiguration.CreateLogger();

                _applicationLogger.ForContext<LogManager>().Information("Application Logger created.");
            }

            return _applicationLogger;
        }
    }

    private readonly bool _isInTestingMode;

    private ILogger _applicationLogger;
}
