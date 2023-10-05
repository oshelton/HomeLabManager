using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace HomeLabManager.Common.Data.Git
{
    /// <summary>
    /// Interface for core Git interactions manager.
    /// </summary>
    public interface IGitDataManager
    {
        /// <summary>
        /// Get whether or not the HomeLabRepoDataPath corresponds to an actual Git Repo.
        /// </summary>
        /// <returns>True if the Directory exists and is an actual Git Repo.</returns>
        bool IsDataPathARepo();

        /// <summary>
        /// Get whether or not the Repo has uncommitted changes.
        /// </summary>
        /// <returns>True if so, false if not.</returns>
        bool RepoHasUncommitedChanges();

        /// <summary>
        /// Get the current changes (if any) in the raw form straight from LibGit2Sharp.
        /// </summary>
        RepositoryStatus GetRepoStatus();

        /// <summary>
        /// Pull the latest changes from the remote repo.
        /// </summary>
        void PullLatestChanges();

        /// <summary>
        /// Commit any uncommitted changes in the working copy.
        /// </summary>
        /// <param name="commitMessage">Message to be associated with the commit.</param>
        /// <returns>True if changes were committed, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">If no commit message is supplied.</exception>
        /// <exception cref="InvalidDataException">If the git config file does not exist or does not contain the expected username and email information.</exception>
        bool CommitAndPushChanges(string commitMessage);
    }
}
