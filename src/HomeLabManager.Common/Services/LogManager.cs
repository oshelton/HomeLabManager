using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using HomeLabManager.Common.Utils;
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
    public ILogger GetApplicationLoggerForContext<T>([CallerMemberName] string callerName = null)
    {
        if (_applicationLogger is null)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose();
            if (Debugger.IsAttached || _isInTestingMode)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Debug(outputTemplate: "APP_LOG: [{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}.{MemberName}] {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                var nowStamp = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss", CultureInfo.InvariantCulture.DateTimeFormat);
                _applicationLogFileName = $@".\logs\log_{nowStamp}.txt";
                loggerConfiguration = loggerConfiguration.WriteTo.File(_applicationLogFileName,
                    outputTemplate: "[{Timestamp:MM-dd-yyyy_HH:mm:ss} {Level:u3}] [{SourceContext:l}.{MemberName}] {Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    shared: true);
            }
            _applicationLogger = loggerConfiguration.CreateLogger();

            _applicationLogger.ForContext<LogManager>().Information("Application Logger created.");
        }

        return _applicationLogger.ForContext<T>().ForContext(MemberNameProp, callerName);
    }

    /// <inheritdoc/>
    public DisposableOperation StartTimedOperation<T>(string timedActionName, [CallerMemberName] string callerName = null)
    {
        if (_performanceLogger is null)
        {
            var loggerConfiguration = new LoggerConfiguration()
                    .Enrich.FromLogContext();
            if (Debugger.IsAttached || _isInTestingMode)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Debug(outputTemplate: "PERFORMANCE_LOG: [{Timestamp:HH:mm:ss} PERF] [{SourceContext:l}.{MemberName}] {Message:lj}");
            }
            else
            {
                loggerConfiguration = loggerConfiguration
                    .WriteTo.File(_applicationLogFileName,
                        outputTemplate: "[{Timestamp:MM-dd-yyyy_HH:mm:ss} PERF] [{SourceContext:l}.{MemberName}] {Message:lj}",
                        shared: true)
                    .WriteTo.File(@".\logs\performance.csv",
                        outputTemplate: "{Timestamp:MM-dd-yyyy_HH:mm:ss}, {SourceContext:l}.{MemberName}, {Message:lj}",
                        rollingInterval: RollingInterval.Day);
            }
            _performanceLogger = loggerConfiguration.CreateLogger();
        }

        var stopwatch = new Stopwatch();
        var actionUniqueId = Guid.NewGuid();
        return new DisposableOperation(
            () =>
            {
                _performanceLogger.ForContext<T>().ForContext(MemberNameProp, callerName).Information("Started {ActionId} - \"{ActionName}\"", actionUniqueId, timedActionName);
                stopwatch.Start();
            },
            () =>
            {
                stopwatch.Stop();
                _performanceLogger.ForContext<T>().ForContext(MemberNameProp, callerName).Information("Completed {ActionId} - \"{ActionName}\" in {ElapsedMs} MS", actionUniqueId, timedActionName, stopwatch.ElapsedMilliseconds);
            }
        );
    }

    private readonly bool _isInTestingMode;

    private const string MemberNameProp = "MemberName";

    private ILogger _applicationLogger;
    private string _applicationLogFileName;
    private ILogger _performanceLogger;
}
