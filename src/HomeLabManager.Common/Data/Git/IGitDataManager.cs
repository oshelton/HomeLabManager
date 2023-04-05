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
        bool IsDataPathARepo();
        bool RepoHasUncommitedChanges();

        RepositoryStatus GetRepoStatus();
    }
}
