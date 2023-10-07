using HomeLabManager.Common.Data.Git;
using LibGit2Sharp;

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// Design time GitDataManager.
/// </summary>
internal sealed class DesignGitDataManager : IGitDataManager
{
    public bool CommitAndPushChanges(string commitMessage) => false;

    public RepositoryStatus GetRepoStatus() => null;

    public bool IsDataPathARepo() => false;

    public void PullLatestChanges() { }

    public bool RepoHasUncommitedChanges() => false;
}
