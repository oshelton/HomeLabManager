using System.Diagnostics;
using Avalonia;
using Avalonia.ReactiveUI;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Services;
using HomeLabManager.Manager.DesignModeServices;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.SharedDialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace HomeLabManager.Manager;

internal class Program
{
    private sealed class RunMode: Splat.IModeDetector 
    {
        public bool? InUnitTestRunner() => false; 
    }

    /// <summary>
    /// Mode to use for service creation.
    /// </summary>
    public enum ServiceMode
    {
        Real,
        Design,
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            s_logger.Error(ex, "Unhandled exception received, application terminating.");
            throw;
        }
    }

    /// <summary>
    /// Avalonia configuration, don't remove; also used by visual designer.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        s_coreConfigurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HomeLabManager.Manager");
        if (!Directory.Exists(s_coreConfigurationDirectory))
            Directory.CreateDirectory(s_coreConfigurationDirectory);

        // Per https://www.reactiveui.net/docs/guidelines/framework/performance-optimization,
        // overriding this to a simpler implementation can help startup performance.
        Splat.ModeDetector.OverrideModeDetector(new RunMode());
        ServiceProvider = BuildServiceProvider(Avalonia.Controls.Design.IsDesignMode ? ServiceMode.Design : ServiceMode.Real);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    /// <summary>
    /// Sets up a test app environment.
    /// </summary>
    public static void BuildTestApp(ServiceOverrides overrides)
    {
        if (overrides is null)
            throw new ArgumentNullException(nameof(overrides));

        IsInTestingMode = true;
        ServiceProvider = BuildServiceProvider(ServiceMode.Real, overrides);
    }

    /// <summary>
    /// Refernce to the IHost responsible for holding onto services.
    /// </summary>
    public static IHost ServiceProvider { get; private set; }

    /// <summary>
    /// Whether or not the app is in testing mode.
    /// </summary>
    public static bool IsInTestingMode { get; private set; }

    private static IHost BuildServiceProvider(ServiceMode mode, ServiceOverrides overrides = null)
    {
        overrides = overrides ?? new ServiceOverrides();

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ILogManager>(provider => new LogManager(IsInTestingMode));

                switch (mode)
                {
                    case ServiceMode.Real:
                        // Add Data Services.
                        services.AddSingleton(provider => overrides.CoreConfigurationManagerServiceBuilder?.Invoke() ?? new CoreConfigurationManager(s_coreConfigurationDirectory, provider.GetService<ILogManager>()));
                        services.AddSingleton(provider => overrides.ServerDataManagerServiceBuilder?.Invoke() ?? new ServerDataManager(provider.GetService<ICoreConfigurationManager>(), provider.GetService<ILogManager>()));
                        
                        // Add non-data services.
                        services.AddSingleton(provider => overrides.NavigationServiceBuilder?.Invoke() ?? new NavigationService(provider.GetService<ILogManager>()));
                        services.AddSingleton(provider => overrides.SharedDialogsServiceBuilder?.Invoke() ?? new SharedDialogsService(provider.GetService<ILogManager>()));
                        break;
                    case ServiceMode.Design:
                        // Add Data Services.
                        services.AddSingleton<ICoreConfigurationManager>(provider => new DesignCoreConfigurationManager());
                        services.AddSingleton<IServerDataManager>(provider => new DesignServerDataManager());

                        // Add non-data services.
                        services.AddSingleton<INavigationService>(provider => new DesignNavigationService());
                        services.AddSingleton(provider => new SharedDialogsService(new LogManager(true))); // No specific design time service.
                        break;
                }
            })
            .Build();

        Debug.Assert(host.Services.GetService<ICoreConfigurationManager>() is not null);
        Debug.Assert(host.Services.GetService<IServerDataManager>() is not null);
        Debug.Assert(host.Services.GetService<INavigationService>() is not null);
        Debug.Assert(host.Services.GetService<ILogManager>() is not null);

        s_logger = host.Services.GetService<ILogManager>().ApplicationLogger.ForContext<Program>();

        s_logger.ForCaller().Information("IHost built; services now available.");

        return host;
    }

    private static ILogger s_logger;
    private static string s_coreConfigurationDirectory;
}
