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
    public App() => _logger = Program.ServiceProvider.Services.GetService<ILogManager>().ApplicationLogger.ForContext<App>();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        _logger.ForCaller().Information("Loaded application XAML");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private readonly ILogger _logger;
}
