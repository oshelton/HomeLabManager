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
            if (coreConfigurationManager is null)
                throw new ArgumentNullException(nameof(coreConfigurationManager));
            _logManager = logManager?.CreateContextualizedLogManager<GitDataManager>() ?? throw new ArgumentNullException(nameof(logManager));
            _logManager.GetApplicationLogger().Information("Created");

            _currentCoreConfiguration = coreConfigurationManager.GetActiveCoreConfiguration();
            coreConfigurationManager.CoreConfigurationUpdated.Subscribe(config => _currentCoreConfiguration = config.IsActive ? config : _currentCoreConfiguration);
        }

        /// <inheritdoc />
        public bool IsDataPathARepo()
        {
            var logger = _logManager.GetApplicationLogger();
            var repoPath = _currentCoreConfiguration.HomeLabRepoDataPath;
            if (string.IsNullOrWhiteSpace(repoPath) || !Directory.Exists(repoPath))
            {
                logger.Warning("Non-existent path available to IsDataPathARepo");
                return false;
            }

            var isValid = Repository.IsValid(repoPath);
            logger.Information("Getting repo valid status of \"{RepoPath}\": {IsValid}", repoPath, isValid);

            return isValid;
        }

        /// <inheritdoc />
        public bool RepoHasUncommitedChanges()
        {
            var repoPath = _currentCoreConfiguration.HomeLabRepoDataPath;

            using var repo = new Repository(repoPath);
            var isDirty = repo.RetrieveStatus().IsDirty;
            _logManager.GetApplicationLogger().Information("Getting dirty status of \"{RepoPath}\": {IsDirty}", repoPath, isDirty);

            return isDirty;
        }

        /// <inheritdoc />
        public RepositoryStatus GetRepoStatus()
        {
            var repoPath = _currentCoreConfiguration.HomeLabRepoDataPath!;

            using var repo = new Repository(repoPath);
            _logManager.GetApplicationLogger().Information("Getting status status of \"{RepoPath}\"", repoPath);

            return repo.RetrieveStatus();
        }

        /// <inheritdoc />
        public void PullLatestChanges()
        {
            var repoPath = _currentCoreConfiguration.HomeLabRepoDataPath!;

            _logManager.GetApplicationLogger().Information("Pulling latest changes for \"{RepoPath}\"", repoPath);

            using var _ = _logManager.StartTimedOperation("Pulling Latest Git Changes");
            using var repo = new Repository(repoPath);
            Commands.Pull(repo, CreateGitSignature(_currentCoreConfiguration.GitConfigFilePath, _logManager), new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = (string url, string usernameFromUrl, SupportedCredentialTypes types) =>
                    {
                        return new UsernamePasswordCredentials
                        {
                            Username = _currentCoreConfiguration.GithubUserName,
                            Password = _currentCoreConfiguration.GithubPat,
                        };
                    }
                }
            });
        }

        /// <inheritdoc />
        public bool CommitAndPushChanges(string commitMessage)
        {
            if (string.IsNullOrWhiteSpace(commitMessage))
                throw new ArgumentNullException(nameof(commitMessage));

            var logger = _logManager.GetApplicationLogger();

            var repoPath = _currentCoreConfiguration.HomeLabRepoDataPath;

            using var _ = _logManager.StartTimedOperation("Committing and Pushing Changes");
            using var repo = new Repository(repoPath);
            if (repo.RetrieveStatus().IsDirty)
            {
                logger.Information("Committing and pushing changes for \"{RepoPath}\"", repoPath);

                Commands.Stage(repo, "*");
                var signature = CreateGitSignature(_currentCoreConfiguration.GitConfigFilePath, _logManager);
                repo.Commit(commitMessage, signature, signature);

                var currentBranch = repo.Head;
                repo.Network.Push(currentBranch, new PushOptions
                {
                    CredentialsProvider = CreateGithubCredentialsHandler(_currentCoreConfiguration.GithubUserName, _currentCoreConfiguration.GithubPat, _logManager)
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
                var gitInfo = File.ReadAllLines(gitConfigFilePath)
                    .Select(line => line.Trim())
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

        private readonly ContextAwareLogManager<GitDataManager> _logManager;

        private CoreConfigurationDto _currentCoreConfiguration;
    }
}
