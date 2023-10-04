using Avalonia.Controls.Documents;

namespace HomeLabManager.Manager.Windows.MainWindowComponents.GitStatusIndicator;

/// <summary>
/// Display Mode of the Git Status Indicator.
/// </summary>
public enum GitStatusIndicatorDisplayMode
{
    NoRepoPath,
    NoValidGitRepo,
    NoChanges,
    UncommittedChanges,
}

/// <summary>
/// Interface for Git Status Indicator View Models.
/// </summary>
internal interface IGitStatusIndicatorViewModel
{
    /// <summary>
    /// Current display mode of the indicator.
    /// </summary>
    GitStatusIndicatorDisplayMode CurrentDisplayMode { get; }

    /// <summary>
    /// Human readable form of any uncommitted changes.
    /// </summary>
    IReadOnlyList<string> UncommittedChanges { get; }

    /// <summary>
    /// Get whether or not changes can be committed.
    /// </summary>
    bool CanCommitChanges { get; }

    /// <summary>
    /// Get whether or not changes are being committed.
    /// </summary>
    bool IsCommittingChanges { get; }
}
