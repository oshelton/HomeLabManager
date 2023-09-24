using System.Reactive.Linq;
using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git;
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

        // Set up the timer for regularly polling the status of the git data. 
        _pollingObserver = Observable.Timer(TimeSpan.FromSeconds(2))
            .Subscribe(async _ => await UpdateGitStatus().ConfigureAwait(true));
    }

    /// <inheritdoc />
    public GitStatusIndicatorDisplayMode CurrentDisplayMode
    {
        get => _currentDisplayMode;
        private set => this.RaiseAndSetIfChanged(ref _currentDisplayMode, value);
    }

    /// <summary>
    /// Update the git data status.
    /// </summary>
    private async Task UpdateGitStatus()
    {
        var displayMode = GitStatusIndicatorDisplayMode.NoRepoPath;
        await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(_coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath))
                displayMode = GitStatusIndicatorDisplayMode.NoRepoPath;
            else if (!_gitDataManager.IsDataPathARepo())
                displayMode = GitStatusIndicatorDisplayMode.NoValidGitRepo;
            else if (!_gitDataManager.RepoHasUncommitedChanges())
                displayMode = GitStatusIndicatorDisplayMode.NoChanges;
            else
                displayMode = GitStatusIndicatorDisplayMode.UncommittedChanges;
        }).ConfigureAwait(false);

        DispatcherHelper.PostToUIThreadIfNecessary(() => CurrentDisplayMode = displayMode, DispatcherPriority.Normal);
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly IGitDataManager _gitDataManager;
    private readonly IDisposable _pollingObserver;

    private GitStatusIndicatorDisplayMode _currentDisplayMode;
}
