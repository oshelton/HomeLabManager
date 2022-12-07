using HomeLabManager.BackupTool;

namespace HomeLabManager.BackupTool;

public static class Program
{
    public static async Task Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            })
            .Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}
