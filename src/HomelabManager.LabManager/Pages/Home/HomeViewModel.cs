using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home;

public enum HomeDisplayMode
{
    NoRepoPath,
    RepoPathDoesNotExist,
    IsLoading,
    NoServers,
    HasServers,
}

/// <summary>
/// Home Page View Model.
/// </summary>
public sealed class HomeViewModel : PageBaseViewModel<HomeViewModel>
{
    public HomeViewModel(): base()
    {
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _coreConfigurationManager = Program.ServiceProvider.Services.GetService<ICoreConfigurationManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

        if (Avalonia.Controls.Design.IsDesignMode)
        {
            var mode = new Random().NextInt64(0, 5);
            switch (mode)
            {
                case 0:
                    CurrentDisplayMode = HomeDisplayMode.NoRepoPath;
                    break;
                case 1:
                    CurrentDisplayMode = HomeDisplayMode.RepoPathDoesNotExist;
                    break;
                case 2:
                    CurrentDisplayMode = HomeDisplayMode.IsLoading;
                    break;
                case 3:
                    CurrentDisplayMode = HomeDisplayMode.NoServers;
                    break;
                case 4:
                    _servers = _serverDataManager.GetServers()
                        .Select(x => new ServerViewModel(x))
                        .OrderBy(x => x.DisplayIndex)
                        .ToArray();
                    CurrentDisplayMode = HomeDisplayMode.HasServers;
                    break;
            }
        }
    }

    public override string Title => "Home";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not HomeNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

        var logger = LogManager.GetApplicationLogger();

        // Confirm there is a repo data path and that it exists.
        var coreConfig = _coreConfigurationManager.GetCoreConfiguration();

        if (string.IsNullOrEmpty(coreConfig.HomeLabRepoDataPath))
        {
            logger.Information("No home lab repo data path specified");
            CurrentDisplayMode = HomeDisplayMode.NoRepoPath;
            return;
        }

        if (!Directory.Exists(coreConfig.HomeLabRepoDataPath))
        {
            logger.Information("Home lab repo data path \"{RepoPath}\" does not exist", coreConfig.HomeLabRepoDataPath);
            CurrentDisplayMode = HomeDisplayMode.RepoPathDoesNotExist;
            return;
        }

        // Load Servers.
        logger.Information("Loading server information");
        CurrentDisplayMode = HomeDisplayMode.IsLoading;

        var servers = await Task.Run(() =>
            _serverDataManager.GetServers().Select(x => new ServerViewModel(x))
                .OrderBy(x => x.DisplayIndex)
                .ToArray()
            ).ConfigureAwait(false);

        logger.Information("Loaded \"{ServerCount}\" servers", servers.Length);

        DispatcherHelper.PostToUIThreadIfNecessary(() =>
        {
            Servers = servers;
            CurrentDisplayMode = (servers?.Length ?? 0) != 0 ? HomeDisplayMode.HasServers : HomeDisplayMode.NoServers;
        }, DispatcherPriority.Input);
    }

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    public async Task NavigateToServerListing() => await _navigationService!.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(false);

    public async Task NavigateToSettings() => await _navigationService!.NavigateTo(new SettingsNavigationRequest()).ConfigureAwait(false);

    /// <summary>
    /// Current display mode.
    /// </summary>
    public HomeDisplayMode CurrentDisplayMode
    {
        get => _currentDisplayMode;
        private set => this.RaiseAndSetIfChanged(ref _currentDisplayMode, value);
    }

    /// <summary>
    /// Collection of servers.
    /// </summary>
    public IReadOnlyList<ServerViewModel> Servers
    {
        get => _servers;
        private set => this.RaiseAndSetIfChanged(ref _servers, value);
    }

    protected override void Dispose(bool isDisposing) { }

    private readonly IServerDataManager _serverDataManager;
    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly INavigationService _navigationService;

    private HomeDisplayMode _currentDisplayMode;
    private IReadOnlyList<ServerViewModel> _servers;
}
