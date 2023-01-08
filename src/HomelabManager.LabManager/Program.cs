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
    public static IHost? ServiceProvider { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        CoreConfigurationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HomeLabManager.Manager");
        if (!Directory.Exists(CoreConfigurationDirectory))
            Directory.CreateDirectory(CoreConfigurationDirectory);

        ServiceProvider = BuildServiceProvider();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    }

    private static IHost BuildServiceProvider()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var isInDesignMode = Avalonia.Controls.Design.IsDesignMode;

                // Add data servers.
                services.AddSingleton<ICoreConfigurationManager>(provider => isInDesignMode 
                    ? new DesignCoreConfigurationManager() 
                    : new CoreConfigurationManager(CoreConfigurationDirectory!));
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

    private static string? CoreConfigurationDirectory { get; set; }
}
