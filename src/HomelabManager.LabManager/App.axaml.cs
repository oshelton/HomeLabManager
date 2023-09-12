using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HomeLabManager.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HomeLabManager.Manager;

/// <summary>
/// Core application class.
/// </summary>
public partial class App : Application
{
    public App() => _logManager = Program.ServiceProvider.Services.GetService<ILogManager>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        _logManager.GetApplicationLoggerForContext<App>().Information("Loaded application XAML");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private readonly ILogManager _logManager;
}
