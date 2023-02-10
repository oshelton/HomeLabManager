using Avalonia.Interactivity;
using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home
{
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
    public sealed class HomeViewModel : PageBaseViewModel
    {
        public HomeViewModel()
        {
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                _serverDataManager = Program.ServiceProvider!.Services.GetService<IServerDataManager>();
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
                        _servers = _serverDataManager!.GetServers().Select(x => new ServerViewModel(x)).ToArray();
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

            // Confirm there is a repo data path and that it exists.
            var coreConfigurationManager = Program.ServiceProvider!.Services.GetService<ICoreConfigurationManager>()!;
            var coreConfig = coreConfigurationManager.GetCoreConfiguration();

            if (string.IsNullOrEmpty(coreConfig.HomeLabRepoDataPath))
            {
                CurrentDisplayMode = HomeDisplayMode.NoRepoPath;
                return;
            }

            if (!Directory.Exists(coreConfig.HomeLabRepoDataPath))
            {
                CurrentDisplayMode = HomeDisplayMode.RepoPathDoesNotExist;
                return;
            }

            _serverDataManager = Program.ServiceProvider!.Services.GetService<IServerDataManager>();
            _navigationService = Program.ServiceProvider!.Services.GetService<INavigationService>();

            // Load Servers.
            CurrentDisplayMode = HomeDisplayMode.IsLoading;

            IReadOnlyList<ServerViewModel>? servers = null;
            await Task.Run(async () =>
            {
                servers = _serverDataManager!.GetServers().Select(x => new ServerViewModel(x)).ToArray();
            }).ConfigureAwait(false);

            DispatcherHelper.PostToUIThread(() =>
            {
                Servers = servers;
                CurrentDisplayMode = (servers?.Count ?? 0) != 0 ? HomeDisplayMode.HasServers : HomeDisplayMode.NoServers;
            }, DispatcherPriority.Input);
        }

        public override Task<bool> TryNavigateAway() => Task.FromResult(true);

        public async Task NavigateToSettings() => await _navigationService!.NavigateTo(new SettingsNavigationRequest()).ConfigureAwait(false);

        public HomeDisplayMode CurrentDisplayMode
        {
            get => _currentDisplayMode;
            private set => this.RaiseAndSetIfChanged(ref _currentDisplayMode, value);
        }

        /// <summary>
        /// Collection of servers.
        /// </summary>
        public IReadOnlyList<ServerViewModel>? Servers
        {
            get => _servers;
            private set => this.RaiseAndSetIfChanged(ref _servers, value);
        }

        private IServerDataManager? _serverDataManager;
        private INavigationService? _navigationService;

        private HomeDisplayMode _currentDisplayMode;
        private IReadOnlyList<ServerViewModel>? _servers;
    }
}
