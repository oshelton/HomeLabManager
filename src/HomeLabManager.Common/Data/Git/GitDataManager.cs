using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using HomeLabManager.Common.Data.CoreConfiguration;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

[assembly: InternalsVisibleTo("HomeLabManager.ConfigurationTests")]

namespace HomeLabManager.Common.Data.Git
{
    /// <summary>
    /// Class for interacting with Git Data.
    /// </summary>
    public sealed class GitDataManager: IGitDataManager
    {
        /// <summary>
        /// Construct a new GitDataManager
        /// </summary>
        public GitDataManager(ICoreConfigurationManager coreConfigurationManager) 
        {
            if (coreConfigurationManager is null)
                throw new ArgumentNullException(nameof(coreConfigurationManager));

            _coreConfigurationManager = coreConfigurationManager;
        }

        /// <summary>
        /// Get whether or not the HomeLabRepoDataPath corresponds to an actual Git Repo.
        /// </summary>
        /// <returns>True if the Directory exists and is an actual Git Repo.</returns>
        public bool IsDataPathARepo()
        {
            var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath;
            if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
                return false;

            return Repository.IsValid(repoPath);
        }

        /// <summary>
        /// Get whether or not the Repo has uncommitted changes.
        /// </summary>
        /// <returns>True if so, false if not.</returns>
        public bool RepoHasUncommitedChanges()
        {
            var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;

            using var repo = new Repository(repoPath);
            return repo.RetrieveStatus().IsDirty;
        }

        /// <summary>
        /// Get the current changes (if any) in the raw form straight from LibGit2Sharp.
        /// </summary>
        public RepositoryStatus GetRepoStatus()
        {
            var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;

            using var repo = new Repository(repoPath);
            return repo.RetrieveStatus();
        }

        /// <summary>
        /// Pull the latest changes from the remote repo.
        /// </summary>
        public void PullLatestChanges()
        {
            var coreConfig = _coreConfigurationManager.GetCoreConfiguration();
            var repoPath = coreConfig.HomeLabRepoDataPath!;

            using var repo = new Repository(repoPath);
            Commands.Pull(repo, CreateGitSignature(coreConfig.GitConfigFilePath), new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = (string url, string usernameFromUrl, SupportedCredentialTypes types) =>
                    {
                        return new UsernamePasswordCredentials
                        {
                            Username = coreConfig.GithubUserName,
                            Password = coreConfig.GithubPat,
                        };
                    }
                }
            });
        }

        /// <summary>
        /// Commit any uncommitted changes in the working copy.
        /// </summary>
        /// <param name="commitMessage">Message to be associated with the commit.</param>
        /// <returns>True if changes were committed, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">If no commit message is supplied.</exception>
        /// <exception cref="InvalidDataException">If the git config file does not exist or does not contain the expected username and email information.</exception>
        public bool CommitAndPushChanges(string commitMessage)
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
                throw new ArgumentNullException(nameof(commitMessage));

            var coreConfig = _coreConfigurationManager.GetCoreConfiguration();
            var repoPath = coreConfig.HomeLabRepoDataPath!;

            using var repo = new Repository(repoPath);
            if (repo.RetrieveStatus().IsDirty)
            {
                Commands.Stage(repo, "*");
                var signature = CreateGitSignature(coreConfig.GitConfigFilePath);
                repo.Commit(commitMessage, signature, signature);

                var currentBranch = repo.Head;
                repo.Network.Push(currentBranch, new PushOptions
                {
                    CredentialsProvider = CreateGithubCredentialsHandler(coreConfig.GithubUserName, coreConfig.GithubPat)
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create a Git signature based on the username and email in the git config file refferenced by the core config.
        /// </summary>
        internal static Signature CreateGitSignature(string? gitConfigFilePath)
        {
            if (!File.Exists(gitConfigFilePath))
                throw new InvalidDataException($"Cannot commit changes without a valid Git configuration file; Path: {gitConfigFilePath}.");

            var gitInfo = File.ReadAllText(gitConfigFilePath)
                .Split("\\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Replace("\\t", "", StringComparison.InvariantCultureIgnoreCase).Trim())
                .Where(line => line.StartsWith("name", true, CultureInfo.InvariantCulture) || line.StartsWith("email", true, CultureInfo.InvariantCulture))
                .Select(line => line.Split('=', StringSplitOptions.TrimEntries))
                .ToDictionary(lineParts => lineParts[0], lineParts => lineParts[1]);

            if (gitInfo.Count != 2)
                throw new InvalidDataException($"Contents of Git configuration file '{gitConfigFilePath}' does not contain the username and email information.");

            var userName = gitInfo["name"];
            var email = gitInfo["email"];

            return new Signature(userName, email, DateTime.Now);
        }

        /// <summary>
        /// Create a Github Credentials Provider.
        /// </summary>
        internal static CredentialsHandler CreateGithubCredentialsHandler(string? githubUserName, string? githubPat)
        {
            return (string url, string usernameFromUrl, SupportedCredentialTypes types) =>
            {
                return new UsernamePasswordCredentials
                {
                    Username = githubUserName,
                    Password = githubPat
                };
            };
        }

        private readonly ICoreConfigurationManager _coreConfigurationManager;
    }
}
