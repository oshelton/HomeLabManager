using HomeLabManager.Common.Data.Git;
using LibGit2Sharp;

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// Design time GitDataManager.
/// </summary>
internal sealed class DesignGitDataManager : IGitDataManager
{
    public RepositoryStatus GetRepoStatus() => null;

    public bool IsDataPathARepo() => false;

    public bool RepoHasUncommitedChanges() => false;
}
