namespace HomeLab.BackupTool;

public class Worker : BackgroundService
{
    public Worker(ILogger<Worker> logger)
    {
        m_logger = logger;
        m_logWorkerStarted = LoggerMessage.Define<DateTimeOffset>(LogLevel.Information, new EventId(0, "Worker Started"), "Worker running at: {Time}");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            m_logWorkerStarted(m_logger, DateTimeOffset.Now, null);
            await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
        }
    }

    private readonly ILogger<Worker> m_logger;
    private readonly Action<ILogger, DateTimeOffset, Exception?> m_logWorkerStarted;
}
