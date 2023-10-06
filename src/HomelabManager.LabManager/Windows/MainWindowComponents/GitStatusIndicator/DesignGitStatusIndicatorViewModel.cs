using Avalonia.Controls.Documents;
using ReactiveUI;

namespace HomeLabManager.Manager.Windows.MainWindowComponents.GitStatusIndicator;

/// <summary>
/// Design version of the GitStatusIndicatorViewModel.
/// </summary>
internal sealed class DesignGitStatusIndicatorViewModel : ReactiveObject, IGitStatusIndicatorViewModel
{
    /// <inheritdoc />
    public GitStatusIndicatorDisplayMode CurrentDisplayMode { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<string> UncommittedChanges { get; } = new[]
    {
        "Change 1",
        "Change 2",
        "Change 3"
    };

    /// <inheritdoc />
    public string DefaultCommitMessageTitle { get; } = "Home Lab Manager Changes Committed";

    /// <inheritdoc />
    public string CustomCommitMessageTitle { get; set; }

    /// <inheritdoc />
    public bool CanCommitChanges { get; set; }

    /// <inheritdoc />
    public bool IsCommittingChanges { get; set; }
}
