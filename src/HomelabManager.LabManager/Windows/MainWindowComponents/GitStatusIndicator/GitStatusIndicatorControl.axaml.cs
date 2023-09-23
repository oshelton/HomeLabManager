using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace HomeLabManager.Manager.Windows.MainWindowComponents.GitStatusIndicator;

public partial class GitStatusIndicatorControl : TemplatedControl
{
    public GitStatusIndicatorControl()
    {
        InitializeComponent();

        if (!Avalonia.Controls.Design.IsDesignMode)
            DataContext = new GitStatusIndicatorViewModel();
        else
            DataContext = new DesignGitStatusIndicatorViewModel() { CurrentDisplayMode = GitStatusIndicatorDisplayMode.NoRepoPath };
    }
}
