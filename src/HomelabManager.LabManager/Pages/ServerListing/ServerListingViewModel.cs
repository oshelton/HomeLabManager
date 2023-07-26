using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using Avalonia.Collections;
using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
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
    public ServerListingViewModel()
    {
        _coreConfigurationManager = Program.ServiceProvider.Services.GetService<ICoreConfigurationManager>();
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

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
                    _servers = new AvaloniaList<ServerViewModel>(_serverDataManager.GetServers().Select(x => new ServerViewModel(x)).ToArray());
                    CurrentDisplayMode = ServerListingDisplayMode.HasServers;
                    break;
            }
        }

        _disposables = new CompositeDisposable();

        CreateNewServerHostCommand = ReactiveCommand.CreateFromTask<int?>(CreateNewServerHost)
            .DisposeWith(_disposables);
    }

    public override string Title => "Server Listing";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not ServerListingNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

        // Load Servers.
        CurrentDisplayMode = ServerListingDisplayMode.IsLoading;

        AvaloniaList<ServerViewModel> servers = null;
        await Task.Run(async () =>
        {
            servers = new AvaloniaList<ServerViewModel>(_serverDataManager.GetServers()
                .Select(x => new ServerViewModel(x))
                .OrderBy(x => x.DisplayIndex));
        }).ConfigureAwait(true);

        Servers = servers;
        CurrentDisplayMode = (servers?.Count ?? 0) != 0 ? ServerListingDisplayMode.HasServers : ServerListingDisplayMode.NoServers;
    }

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    public ReactiveCommand<int?, Unit> CreateNewServerHostCommand { get; }

    /// <summary>
    /// Current display mode.
    /// </summary>
    public ServerListingDisplayMode CurrentDisplayMode
    {
        get => _currentDisplayMode;
        private set => this.RaiseAndSetIfChanged(ref _currentDisplayMode, value);
    }

    /// <summary>
    /// Collection of servers.
    /// </summary>
    public virtual IAvaloniaReadOnlyList<ServerViewModel> Servers
    {
        get => _servers;
        private set => this.RaiseAndSetIfChanged(ref _servers, (AvaloniaList<ServerViewModel>)value);
    }

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

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly IServerDataManager _serverDataManager;
    private readonly INavigationService _navigationService;

    private readonly CompositeDisposable _disposables;

    // Property backing fields.
    private ServerListingDisplayMode _currentDisplayMode;
    private AvaloniaList<ServerViewModel> _servers;
}
