using Avalonia.Controls;

namespace HomeLabManager.Manager;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        Instance = this;
    }
}
