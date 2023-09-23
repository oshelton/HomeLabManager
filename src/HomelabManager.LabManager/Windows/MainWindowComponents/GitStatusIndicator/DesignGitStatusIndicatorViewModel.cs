using ReactiveUI;

namespace HomeLabManager.Manager.Windows.MainWindowComponents.GitStatusIndicator;

/// <summary>
/// Design version of the GitStatusIndicatorViewModel.
/// </summary>
internal sealed class DesignGitStatusIndicatorViewModel : ReactiveObject, IGitStatusIndicatorViewModel
{
    /// <inheritdoc />
    public GitStatusIndicatorDisplayMode CurrentDisplayMode { get; set; }
}
