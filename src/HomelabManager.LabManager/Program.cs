using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git;
using HomeLabManager.Manager.DesignModeServices;
using HomeLabManager.Manager.Services.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HomeLabManager.Manager;

internal class Program
{
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

        ServiceProvider = BuildServiceProvider(false);

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
        ServiceProvider = BuildServiceProvider(true);
    }

    /// <summary>
    /// Refernce to the IHost responsible for holding onto services.
    /// </summary>
    public static IHost? ServiceProvider { get; private set; }

    /// <summary>
    /// Whether or not the app is in testing mode.
    /// </summary>
    public static bool IsInTestingMode { get; private set; }

    private static IHost BuildServiceProvider(bool forceDesignMode)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var isInDesignMode = Avalonia.Controls.Design.IsDesignMode || forceDesignMode;

                // Add data servers.
                services.AddSingleton<ICoreConfigurationManager>(provider => isInDesignMode 
                    ? new DesignCoreConfigurationManager() 
                    : new CoreConfigurationManager(s_coreConfigurationDirectory!));
                services.AddTransient<IServerDataManager>(provider => isInDesignMode 
                    ? new DesignServerDataManager() 
                    : new ServerDataManager(provider.GetService<ICoreConfigurationManager>()!));

                // Add non-data related services.
                services.AddSingleton<INavigationService>(provider => isInDesignMode
                    ? new DesignNavigationService()
                    : new NavigationService());
            })
            .Build();
    }

    private static string? s_coreConfigurationDirectory;
}
