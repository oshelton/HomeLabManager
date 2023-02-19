using System.Diagnostics;
using Avalonia;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.DesignModeServices;
using HomeLabManager.Manager.Services.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeLabManager.Manager;

internal class Program
{
    /// <summary>
    /// Mode to use for service creation.
    /// </summary>
    public enum ServiceMode
    {
        Real,
        Design,
        Testing,
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        s_coreConfigurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HomeLabManager.Manager");
        if (!Directory.Exists(s_coreConfigurationDirectory))
            Directory.CreateDirectory(s_coreConfigurationDirectory);

        ServiceProvider = BuildServiceProvider(Avalonia.Controls.Design.IsDesignMode ? ServiceMode.Design : ServiceMode.Real);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }

    /// <summary>
    /// Sets up a test app environment.
    /// </summary>
    public static void BuildTestApp()
    {
        IsInTestingMode = true;
        ServiceProvider = BuildServiceProvider(ServiceMode.Testing);
    }

    /// <summary>
    /// Refernce to the IHost responsible for holding onto services.
    /// </summary>
    public static IHost? ServiceProvider { get; private set; }

    /// <summary>
    /// Whether or not the app is in testing mode.
    /// </summary>
    public static bool IsInTestingMode { get; private set; }

    private static IHost BuildServiceProvider(ServiceMode mode)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                switch (mode)
                {
                    case ServiceMode.Real:
                        // Add Data Services.
                        services.AddSingleton<ICoreConfigurationManager>(provider => new CoreConfigurationManager(s_coreConfigurationDirectory!));
                        services.AddSingleton<IServerDataManager>(provider => new ServerDataManager(provider.GetService<ICoreConfigurationManager>()!));
                        
                        // Add non-data services.
                        services.AddSingleton<INavigationService>(provider => new NavigationService());
                        break;
                    case ServiceMode.Design:
                        // Add Data Services.
                        services.AddSingleton<ICoreConfigurationManager>(provider => new DesignCoreConfigurationManager());
                        services.AddSingleton<IServerDataManager>(provider => new DesignServerDataManager());

                        // Add non-data services.
                        services.AddSingleton<INavigationService>(provider => new DesignNavigationService());
                        break;
                    case ServiceMode.Testing:
                        // Add Data Services.
                        services.AddSingleton<ICoreConfigurationManager>(provider => new TestCoreConfigurationManager());
                        services.AddSingleton<IServerDataManager>(provider => new TestServerDataManager());

                        // Add non-data services.
                        services.AddSingleton<INavigationService>(provider => new TestNavigationService());
                        break;
                }
            })
            .Build();

        Debug.Assert(host.Services.GetService<ICoreConfigurationManager>() is not null);
        Debug.Assert(host.Services.GetService<IServerDataManager>() is not null);
        Debug.Assert(host.Services.GetService<INavigationService>() is not null);

        return host;
    }

    private static string? s_coreConfigurationDirectory;
}
