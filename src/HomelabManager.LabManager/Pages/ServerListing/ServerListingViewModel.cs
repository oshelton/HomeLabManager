using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Services;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Services.SharedDialogs;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.ServerListing;

public enum ServerListingDisplayMode
{
    IsLoading,
    NoServers,
    HasServers,
}

/// <summary>
/// Server Listing Page View Model.
/// </summary>
public class ServerListingViewModel : PageBaseViewModel
{
    public ServerListingViewModel() : base()
    {
        _coreConfigurationManager = Program.ServiceProvider.Services.GetService<ICoreConfigurationManager>();
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();
        _sharedDialogsService = Program.ServiceProvider.Services.GetService<ISharedDialogsService>();
        _logManager = Program.ServiceProvider.Services.GetService<ILogManager>();

        if (Avalonia.Controls.Design.IsDesignMode)
        {
            var mode = new Random().NextInt64(0, 3);
            switch (mode)
            {
                case 0:
                    CurrentDisplayMode = ServerListingDisplayMode.IsLoading;
                    break;
                case 1:
                    CurrentDisplayMode = ServerListingDisplayMode.NoServers;
                    break;
                case 2:
                    var servers = _serverDataManager.GetServers();
                    _serverCache.AddOrUpdate(servers.Select(x => new ServerViewModel(x, this, servers.Count)));
                    CurrentDisplayMode = ServerListingDisplayMode.HasServers;
                    break;
            }
        }

        _disposables = new CompositeDisposable();

        CreateNewServerHostCommand = ReactiveCommand.CreateFromTask<int?>(CreateNewServerHost)
            .DisposeWith(_disposables);

        EditServerCommand = ReactiveCommand.CreateFromTask<ServerViewModel>(EditServerHost)
            .DisposeWith(_disposables);

        MoveServerUpCommand = ReactiveCommand.CreateFromTask<ServerViewModel>(MoveServerHostUp)
            .DisposeWith(_disposables);

        MoveServerDownCommand = ReactiveCommand.CreateFromTask<ServerViewModel>(MoveServerHostDown)
            .DisposeWith(_disposables);

        DeleteServerCommand = ReactiveCommand.CreateFromTask<ServerViewModel>(DeleteServerHost)
            .DisposeWith(_disposables);

        var serverListObservable = _serverCache.Connect().Publish();

        serverListObservable.AutoRefresh(server => server.DisplayIndex, propertyChangeThrottle: TimeSpan.FromMilliseconds(10))
            .SortBy(x => x.DisplayIndex)
            .Bind(out _sortedServers)
            .Subscribe()
            .DisposeWith(_disposables);

        _disposables.Add(serverListObservable.Connect());
    }

    public override string Title => "Server Listing";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not ServerListingNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

        var logger = _logManager.GetApplicationLoggerForContext<ServerListingViewModel>();

        // Load Servers.
        CurrentDisplayMode = ServerListingDisplayMode.IsLoading;

        logger.Information("Loading servers");

        await Task.Run(async () =>
        {
            var servers = _serverDataManager.GetServers();
            var serverViewModels = servers.Select(x => new ServerViewModel(x, this, servers.Count)).OrderBy(x => x.DisplayIndex);
            foreach (var server in serverViewModels)
            {
                _disposables.Add(server);
            }
            _serverCache.Clear();
            _serverCache.AddOrUpdate(serverViewModels);
        }).ConfigureAwait(true);

        logger.Information("Loaded servers");

        CurrentDisplayMode = _serverCache.Count != 0 ? ServerListingDisplayMode.HasServers : ServerListingDisplayMode.NoServers;
    }

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    /// <summary>
    /// Command to create a ner server at the specified location.
    /// </summary>
    public ReactiveCommand<int?, Unit> CreateNewServerHostCommand { get; }

    /// <summary>
    /// Command to edit a server.
    /// </summary>
    public ReactiveCommand<ServerViewModel, Unit> EditServerCommand { get; }

    /// <summary>
    /// Command to move a server up in the ordering.
    /// </summary>
    public ReactiveCommand<ServerViewModel, Unit> MoveServerUpCommand { get; }

    /// <summary>
    /// Move server down in the ordering.
    /// </summary>
    public ReactiveCommand<ServerViewModel, Unit> MoveServerDownCommand { get; }

    /// <summary>
    /// Delete server command.
    /// </summary>
    public ReactiveCommand<ServerViewModel, Unit> DeleteServerCommand { get; }

    /// <summary>
    /// Current display mode.
    /// </summary>
    public ServerListingDisplayMode CurrentDisplayMode
    {
        get => _currentDisplayMode;
        private set => this.RaiseAndSetIfChanged(ref _currentDisplayMode, value);
    }

    /// <summary>
    /// Collection of sorted servers.
    /// </summary>
    public virtual ReadOnlyObservableCollection<ServerViewModel> SortedServers => _sortedServers;

    protected override void Dispose(bool isDisposing) => _disposables.Dispose();

    /// <summary>
    /// Create a new Server.
    /// </summary>
    private Task CreateNewServerHost(int? afterIndex = null) => _navigationService.NavigateTo(new CreateEditServerNavigationRequest(true, new ServerHostDto()
    {
        Metadata = new ServerMetadataDto { DisplayName = "New Server" },
        Configuration = new ServerConfigurationDto(),
        DockerCompose = new DockerComposeDto(),
        VMs = Array.Empty<ServerVmDto>(),
    }, afterIndex));

    /// <summary>
    /// Edit an existing server.
    /// </summary>
    private Task EditServerHost(ServerViewModel server)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));

        return _navigationService.NavigateTo(new CreateEditServerNavigationRequest(false, server.ToDto()));
    }

    /// <summary>
    /// Move passed in server up in the ordering.
    /// </summary>
    private Task MoveServerHostUp(ServerViewModel server)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));
        if (server.DisplayIndex == 0)
            throw new ArgumentException($"{nameof(server)} cannot be moved up if it is first!");

        var previous = SortedServers.First(x => x.DisplayIndex == server.DisplayIndex - 1);
        previous.DisplayIndex++;
        server.DisplayIndex--;

        _logManager.GetApplicationLoggerForContext<ServerListingViewModel>().Information("Moving server \"{MoveUp}\" up and server \"{MoveDown}\" down", server.UniqueId, previous.UniqueId);
        return Task.Run(() =>
        {
            _serverDataManager.AddUpdateServer(previous.ToDto());
            _serverDataManager.AddUpdateServer(server.ToDto());
        });
    }

    /// <summary>
    /// Move passed in server down in the ordering.
    /// </summary>
    private Task MoveServerHostDown(ServerViewModel server)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));
        if (server.DisplayIndex == SortedServers.Count - 1)
            throw new ArgumentException($"{nameof(server)} cannot be moved down if it is last!");

        var next = SortedServers.First(x => x.DisplayIndex == server.DisplayIndex + 1);
        next.DisplayIndex--;
        server.DisplayIndex++;

        _logManager.GetApplicationLoggerForContext<ServerListingViewModel>().Information("Moving server \"{MoveDown}\" down and server \"{MoveUp}\" up", server.UniqueId, next.UniqueId);
        return Task.Run(() =>
        {
            _serverDataManager.AddUpdateServer(server.ToDto());
            _serverDataManager.AddUpdateServer(next.ToDto());
        });
    }

    /// <summary>
    /// Delete an existing server.
    /// </summary>
    private async Task DeleteServerHost(ServerViewModel server)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));

        var logger = _logManager.GetApplicationLoggerForContext<ServerListingViewModel>();

        var continueDeleting = await _sharedDialogsService.ShowSimpleYesNoDialog("Deleting cannot be readily undone outside of Git.\nAre you sure?").ConfigureAwait(true);

        if (!continueDeleting)
        {
            logger.Information("User chose to abort deleting server \"{UniqueID}\"", server.UniqueId);
            return;
        }

        var (dialog, dialogTask) = _sharedDialogsService.ShowSimpleSavingDataDialog("Deleting Server...");

        var toUpdateServers = SortedServers.Where(x => x.DisplayIndex > server.DisplayIndex).ToArray();
        await Task.Run(() =>
        {
            logger.Information("Deleting server \"{UniqueID}\"", server.UniqueId);
            _serverDataManager.DeleteServer(server.ToDto());

            logger.Information("Updating display index of \"{UpdateCount}\" servers after the deleted one", toUpdateServers.Length);
            foreach (var toUpdate in toUpdateServers)
            {
                toUpdate.DisplayIndex--;
                _serverDataManager.AddUpdateServer(toUpdate.ToDto());
            }
        }).ConfigureAwait(true);

        _serverCache.Remove(server);

        dialog?.GetWindow().Close();
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly IServerDataManager _serverDataManager;
    private readonly INavigationService _navigationService;
    private readonly ISharedDialogsService _sharedDialogsService;
    private readonly ILogManager _logManager;

    private readonly CompositeDisposable _disposables;

    // Property backing fields.
    private ServerListingDisplayMode _currentDisplayMode;
    private SourceCache<ServerViewModel, Guid> _serverCache = new SourceCache<ServerViewModel, Guid>(x => x.UniqueId);
    private ReadOnlyObservableCollection<ServerViewModel> _sortedServers;
}
