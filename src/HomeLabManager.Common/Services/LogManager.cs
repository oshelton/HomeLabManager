using System.Diagnostics;
using System.Globalization;
using Serilog;

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
                    .Enrich.FromLogContext();
                if (Debugger.IsAttached || _isInTestingMode)
                {
                    loggerConfiguration = loggerConfiguration.WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}.{MemberName}] {Message:lj}{NewLine}{Exception}");
                }
                else
                {
                    var nowStamp = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss", CultureInfo.InvariantCulture.DateTimeFormat);
                    loggerConfiguration = loggerConfiguration.WriteTo.File($@".\logs\log_{nowStamp}.txt");
                }
                _applicationLogger = loggerConfiguration.CreateLogger();
            }

            return _applicationLogger;
        }
    }

    private readonly bool _isInTestingMode;

    private ILogger _applicationLogger;
}
