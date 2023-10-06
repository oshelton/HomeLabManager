using System.Reactive.Linq;
using Avalonia.Controls.Documents;
using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Windows.MainWindowComponents.GitStatusIndicator;

/// <summary>
/// Class for backing the Git Data Status Indicator on the Main Window.
/// </summary>
public sealed class GitStatusIndicatorViewModel : ReactiveObject, IGitStatusIndicatorViewModel
{
    /// <summary>
    /// Constructor, sset up services and polling.
    /// </summary>
    public GitStatusIndicatorViewModel()
    {
        _coreConfigurationManager = Program.ServiceProvider.Services.GetService<ICoreConfigurationManager>();
        _gitDataManager = Program.ServiceProvider.Services.GetService<IGitDataManager>();
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _logManager = Program.ServiceProvider.Services.GetService<ILogManager>().CreateContextualizedLogManager<GitStatusIndicatorViewModel>();

        // Set up the timer for regularly polling the status of the git data. 
        Observable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2), RxApp.MainThreadScheduler)
            .Subscribe(async _ => await UpdateGitStatus().ConfigureAwait(true));
    }

    public async Task CommitChanges()
    {
        if (!CanCommitChanges)
            throw new InvalidOperationException("Attempting to commit changes when we can't.");

        IReadOnlyList<string> changes = null;
        var title = string.IsNullOrWhiteSpace(CustomCommitMessageTitle) ? CustomCommitMessageTitle : DefaultCommitMessageTitle;
        await DispatcherHelper.PostToUIThreadIfNecessary(() =>
        {
            IsCommittingChanges = true;
            changes = UncommittedChanges.ToArray();
        }, DispatcherPriority.Normal).ConfigureAwait(false);

        await Task.Run(() => _gitDataManager.CommitAndPushChanges($"{title}\n\n{string.Join("\n", changes)}")).ConfigureAwait(false);

        await DispatcherHelper.PostToUIThreadIfNecessary(() =>
        {
            IsCommittingChanges = false;
            CustomCommitMessageTitle = null;
        }, DispatcherPriority.Normal).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public GitStatusIndicatorDisplayMode CurrentDisplayMode
    {
        get => _currentDisplayMode;
        private set => this.RaiseAndSetIfChanged(ref _currentDisplayMode, value);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> UncommittedChanges
    {
        get => _uncommittedChanges;
        private set => this.RaiseAndSetIfChanged(ref _uncommittedChanges, value);
    }

    /// <inheritdoc />
    public string DefaultCommitMessageTitle { get; } = "Home Lab Manager Changes Committed";

    /// <inheritdoc />
    public string CustomCommitMessageTitle 
    {
        get => _customCommitMessageTitle;
        set => this.RaiseAndSetIfChanged(ref _customCommitMessageTitle, value);
    }

    /// <inheritdoc />
    public bool CanCommitChanges
    {
        get => _canCommitChanges;
        set => this.RaiseAndSetIfChanged(ref _canCommitChanges, value);
    }

    /// <inheritdoc />
    public bool IsCommittingChanges
    {
        get => _isCommittingChanges;
        set => this.RaiseAndSetIfChanged(ref _isCommittingChanges, value);
    }

    /// <summary>
    /// Update the git data status.
    /// </summary>
    private async Task UpdateGitStatus()
    {
        if (IsCommittingChanges)
            return;

        var displayMode = GitStatusIndicatorDisplayMode.NoRepoPath;
        IReadOnlyList<string> statusMessages = Array.Empty<string>();
        var canCommit = false;

        using (_logManager.StartTimedOperation("Updating Git Status Indicator Status"))
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(_coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath))
                    displayMode = GitStatusIndicatorDisplayMode.NoRepoPath;
                else if (!_gitDataManager.IsDataPathARepo())
                    displayMode = GitStatusIndicatorDisplayMode.NoValidGitRepo;
                else if (!_gitDataManager.RepoHasUncommitedChanges())
                    displayMode = GitStatusIndicatorDisplayMode.NoChanges;
                else
                {
                    displayMode = GitStatusIndicatorDisplayMode.UncommittedChanges;
                    var changes = _gitDataManager.GetRepoStatus();

                    statusMessages = _serverDataManager.MapChangesToHumanReadableInfo(changes);

                    var coreConfig = _coreConfigurationManager.GetCoreConfiguration();
                    if (!string.IsNullOrEmpty(coreConfig.GithubPat) && !string.IsNullOrEmpty(coreConfig.GithubUserName) && File.Exists(coreConfig.GitConfigFilePath))
                        canCommit = true;
                }
            }).ConfigureAwait(false);
        }

        await DispatcherHelper.PostToUIThreadIfNecessary(() =>
        {
            CurrentDisplayMode = displayMode;
            UncommittedChanges = statusMessages;
            CanCommitChanges = canCommit;
        }, DispatcherPriority.Normal).ConfigureAwait(false);
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly IGitDataManager _gitDataManager;
    private readonly IServerDataManager _serverDataManager;
    private readonly ContextAwareLogManager<GitStatusIndicatorViewModel> _logManager;

    private GitStatusIndicatorDisplayMode _currentDisplayMode;
    private IReadOnlyList<string> _uncommittedChanges;
    private string _customCommitMessageTitle;
    private bool _canCommitChanges;
    private bool _isCommittingChanges;
}
