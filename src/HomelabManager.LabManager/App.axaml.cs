using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HomeLabManager.Common.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HomeLabManager.Manager;

/// <summary>
/// Core application class.
/// </summary>
public partial class App : Application
{
    public App() => _logManager = Program.ServiceProvider.Services.GetService<ILogManager>().CreateContextualizedLogManager<App>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        _logManager.GetApplicationLogger().Information("Loaded application XAML");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private readonly ContextAwareLogManager<App> _logManager;
}
