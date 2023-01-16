using Avalonia.Interactivity;
using Avalonia.Threading;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home
{
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
                var mode = new Random().NextInt64(0, 3);
                switch (mode)
                {
                    case 0:
                        IsLoading = true;
                        break;
                    case 1:
                        _servers = Array.Empty<ServerViewModel>();
                        break;
                    case 2:
                        _servers = _serverDataManager!.GetServers().Select(x => new ServerViewModel(x)).ToArray();
                        break;
                }
                _hasAnyServers = (_servers?.Count ?? 0) != 0;
            }
        }

        public override string Title => "Home";

        public override async Task NavigateTo(INavigationRequest request)
        {
            _serverDataManager = Program.ServiceProvider!.Services.GetService<IServerDataManager>();
            _navigationService = Program.ServiceProvider!.Services.GetService<INavigationService>();

            if (request is not HomeNavigationRequest)
                throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

            IsLoading = true;

            IReadOnlyList<ServerViewModel>? servers = null;
            await Task.Run(async () =>
            {
                servers = _serverDataManager!.GetServers().Select(x => new ServerViewModel(x)).ToArray();
            }).ConfigureAwait(false);

            DispatcherHelper.PostToUIThread(() =>
            {
                IsLoading = false;
                Servers = servers;
                HasAnyServers = (servers?.Count ?? 0) != 0;
            }, DispatcherPriority.Input);
        }

        public async Task NavigateToSettings()
        {
            var nextRand = new Random().Next();
            await _navigationService!.NavigateTo(new SettingsNavigationRequest()).ConfigureAwait(false);
        }

        public override Task<bool> TryNavigateAway() => Task.FromResult(true);

        public bool IsLoading
        {
            get => _isLoading;
            private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public bool HasAnyServers
        {
            get => _hasAnyServers;
            set => this.RaiseAndSetIfChanged(ref _hasAnyServers, value);
        }

        public IReadOnlyList<ServerViewModel>? Servers
        {
            get => _servers;
            private set => this.RaiseAndSetIfChanged(ref _servers, value);
        }

        private IServerDataManager? _serverDataManager;
        private INavigationService? _navigationService;

        private bool _isLoading;
        private bool _hasAnyServers;
        private IReadOnlyList<ServerViewModel>? _servers;
    }
}
