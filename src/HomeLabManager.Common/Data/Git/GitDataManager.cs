using System.Globalization;
using System.Runtime.CompilerServices;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Services.Logging;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Serilog;

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
        public GitDataManager(ICoreConfigurationManager coreConfigurationManager, ILogManager logManager) 
        {
            _coreConfigurationManager = coreConfigurationManager ?? throw new ArgumentNullException(nameof(coreConfigurationManager));

            _logManager = logManager?.CreateContextualizedLogManager<GitDataManager>() ?? throw new ArgumentNullException(nameof(logManager));
            _logManager.GetApplicationLogger().Information("Created");
        }

        /// <summary>
        /// Get whether or not the HomeLabRepoDataPath corresponds to an actual Git Repo.
        /// </summary>
        /// <returns>True if the Directory exists and is an actual Git Repo.</returns>
        public bool IsDataPathARepo()
        {
            var logger = _logManager.GetApplicationLogger();
            var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath;
            if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
            {
                logger.Warning("Non-existent path available to IsDataPathARepo");
                return false;
            }

            var isValid = Repository.IsValid(repoPath);
            logger.Information("Getting repo valid status of \"{RepoPath}\": {IsValid}", repoPath, isValid);

            return isValid;
        }

        /// <summary>
        /// Get whether or not the Repo has uncommitted changes.
        /// </summary>
        /// <returns>True if so, false if not.</returns>
        public bool RepoHasUncommitedChanges()
        {
            var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath;

            using var repo = new Repository(repoPath);
            var isDirty = repo.RetrieveStatus().IsDirty;
            _logManager.GetApplicationLogger().Information("Getting dirty status of \"{RepoPath}\": {IsDirty}", repoPath, isDirty);

            return isDirty;
        }

        /// <summary>
        /// Get the current changes (if any) in the raw form straight from LibGit2Sharp.
        /// </summary>
        public RepositoryStatus GetRepoStatus()
        {
            var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;

            using var repo = new Repository(repoPath);
            _logManager.GetApplicationLogger().Information("Getting status status of \"{RepoPath}\"", repoPath);

            return repo.RetrieveStatus();
        }

        /// <summary>
        /// Pull the latest changes from the remote repo.
        /// </summary>
        public void PullLatestChanges()
        {
            var coreConfig = _coreConfigurationManager.GetCoreConfiguration();
            var repoPath = coreConfig.HomeLabRepoDataPath!;

            _logManager.GetApplicationLogger().Information("Pulling latest changes for \"{RepoPath}\"", repoPath);

            using var _ = _logManager.StartTimedOperation("Pulling Latest Git Changes");
            using var repo = new Repository(repoPath);
            Commands.Pull(repo, CreateGitSignature(coreConfig.GitConfigFilePath, _logManager), new PullOptions
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

            var logger = _logManager.GetApplicationLogger();

            var coreConfig = _coreConfigurationManager.GetCoreConfiguration();
            var repoPath = coreConfig.HomeLabRepoDataPath;

            using var _ = _logManager.StartTimedOperation("Committing and Pushing Changes");
            using var repo = new Repository(repoPath);
            if (repo.RetrieveStatus().IsDirty)
            {
                logger.Information("Committing and pushing changes for \"{RepoPath}\"", repoPath);

                Commands.Stage(repo, "*");
                var signature = CreateGitSignature(coreConfig.GitConfigFilePath, _logManager);
                repo.Commit(commitMessage, signature, signature);

                var currentBranch = repo.Head;
                repo.Network.Push(currentBranch, new PushOptions
                {
                    CredentialsProvider = CreateGithubCredentialsHandler(coreConfig.GithubUserName, coreConfig.GithubPat, _logManager)
                });

                return true;
            }
            else
            {
                logger.Information("No changes to push for \"{RepoPath}\"", repoPath);
                return false;
            }
        }

        /// <summary>
        /// Create a Git signature based on the username and email in the git config file refferenced by the core config.
        /// </summary>
        internal static Signature CreateGitSignature(string gitConfigFilePath, ContextAwareLogManager<GitDataManager> logManager)
        {
            if (!File.Exists(gitConfigFilePath))
                throw new InvalidDataException($"Cannot commit changes without a valid Git configuration file; Path: {gitConfigFilePath}.");

            logManager.GetApplicationLogger().Information("Creating git signature from information in file \"{GitConfigPath}\"", gitConfigFilePath);

            using (logManager.StartTimedOperation("Constructing Signature from Git Config File"))
            {
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
        }

        /// <summary>
        /// Create a Github Credentials Provider.
        /// </summary>
        internal static CredentialsHandler CreateGithubCredentialsHandler(string githubUserName, string githubPat, ContextAwareLogManager<GitDataManager> logManager)
        {
            logManager.GetApplicationLogger().Information("Creating github credentials handler for \"{GithubUserName}\"", githubUserName);

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
        private readonly ContextAwareLogManager<GitDataManager> _logManager;
    }
}
