using System.Reactive.Disposables;
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

        this.WhenAnyValue(x => x.DesignDisplayMode)
            .Subscribe(x =>
            {
                switch (x)
                {
                    case HomeDisplayMode.NoRepoPath:
                        CurrentDisplayMode = HomeDisplayMode.NoRepoPath;
                        break;
                    case HomeDisplayMode.RepoPathDoesNotExist:
                        CurrentDisplayMode = HomeDisplayMode.RepoPathDoesNotExist;
                        break;
                    case HomeDisplayMode.IsLoading:
                        CurrentDisplayMode = HomeDisplayMode.IsLoading;
                        break;
                    case HomeDisplayMode.NoServers:
                        CurrentDisplayMode = HomeDisplayMode.NoServers;
                        break;
                    case HomeDisplayMode.HasServers:
                        _servers = new[]
                        {
                                    new ServerHostDto()
                            {
                                Metadata = new ServerMetadataDto()
                                {
                                    DisplayName = "server 1",
                                    Name = "server1",
                                    IPAddress = "192.168.1.1",
                                    Description = "Server 1 Description",
                                    Kind = ServerKind.Windows,
                                },
                            },
                            new ServerHostDto()
                            {
                                Metadata = new ServerMetadataDto()
                                {
                                    DisplayName = "server 2",
                                    Name = "server2",
                                    IPAddress = "192.168.1.2",
                                    Description = "Server 2 Description",
                                    Kind = ServerKind.StandardLinux,
                                },
                            },
                            new ServerHostDto()
                            {
                                Metadata = new ServerMetadataDto()
                                {
                                    DisplayName = "server 3",
                                    Name = "server3",
                                    IPAddress = "192.168.1.3",
                                    Description = "Server 3 Description",
                                    Kind = ServerKind.TrueNasScale,
                                },
                                VMs = new[]
                                {
                                    new ServerVmDto()
                                    {
                                        Metadata = new ServerMetadataDto()
                                        {
                                            DisplayName = "VM 1",
                                            Name = "vm1",
                                            IPAddress = "192.168.1.4",
                                            Description = "VM 1 Description",
                                            Kind = ServerKind.StandardLinux,
                                        },
                                    },
                                    new ServerVmDto()
                                    {
                                        Metadata = new ServerMetadataDto()
                                        {
                                            DisplayName = "VM 2",
                                            Name = "vm2",
                                            IPAddress = "192.168.1.5",
                                            Description = "VM 2 Description",
                                            Kind = ServerKind.StandardLinux,
                                        },
                                    },
                                }
                            },
                        }.Select(y => new ServerViewModel(y)).ToArray();
                        CurrentDisplayMode = HomeDisplayMode.HasServers;
                        break;
                }
            })
            .DisposeWith(_disposables);
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

        await DispatcherHelper.PostToUIThreadIfNecessary(() =>
        {
            Servers = servers;
            CurrentDisplayMode = (servers?.Length ?? 0) != 0 ? HomeDisplayMode.HasServers : HomeDisplayMode.NoServers;
        }, DispatcherPriority.Input).ConfigureAwait(false);
    }

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    public async Task NavigateToServerListing() => await _navigationService!.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(false);

    public async Task NavigateToSettings() => await _navigationService!.NavigateTo(new SettingsNavigationRequest()).ConfigureAwait(false);

    /// <summary>
    /// Display mode to use at design time.
    /// </summary>
    public HomeDisplayMode DesignDisplayMode
    {
        get => _designDisplayMode;
        set => this.RaiseAndSetIfChanged(ref _designDisplayMode, value);
    }

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

    protected override void Dispose(bool isDisposing) => _disposables.Dispose();

    private readonly IServerDataManager _serverDataManager;
    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly INavigationService _navigationService;
    private readonly CompositeDisposable _disposables = new();

    private HomeDisplayMode _designDisplayMode;
    private HomeDisplayMode _currentDisplayMode;
    private IReadOnlyList<ServerViewModel> _servers;
}
