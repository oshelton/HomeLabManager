using Avalonia.Controls;
using HomeLabManager.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HomeLabManager.Manager;

public sealed partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }

    public MainWindow()
    {
        var logger = Program.ServiceProvider.Services.GetService<ILogManager>().ApplicationLogger.ForContext<MainWindow>();

        InitializeComponent();

        Instance = this;

        logger.ForCaller().Information("Initialized main window");
    }
}
