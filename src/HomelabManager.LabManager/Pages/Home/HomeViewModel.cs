using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using HomeLabManager.Common.Data.Git;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home
{
    /// <summary>
    /// Home Page View Model.
    /// </summary>
    public sealed class HomeViewModel: PageBaseViewModel
    {
        public HomeViewModel()
        {
            _serverDataManager = Program.ServiceProvider!.Services.GetService<IServerDataManager>()!;

            if (Avalonia.Controls.Design.IsDesignMode)
                _servers = _serverDataManager.GetServers().Select(x => new ServerViewModel(x)).ToArray();
        }

        public override string Title => "Home";

        public override async void NavigateTo(INavigationRequest request)
        {
            if (request is not HomeNavigationRequest)
                throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

            IsLoading = true;

            IReadOnlyList<ServerViewModel>? servers = null;
            await Task.Run(async () =>
            {
                servers = _serverDataManager.GetServers().Select(x => new ServerViewModel(x)).ToArray();
            }).ConfigureAwait(false);

            Dispatcher.UIThread.Post(() =>
            {
                IsLoading = false;
                Servers = servers;
            }, DispatcherPriority.Input);
        }

        public override Task<bool> TryNavigateAway() => Task.FromResult(true);

        public bool IsLoading
        {
            get => _isLoading;
            private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public IReadOnlyList<ServerViewModel>? Servers
        {
            get => _servers;
            private set => this.RaiseAndSetIfChanged(ref _servers, value);
        }

        private readonly IServerDataManager _serverDataManager;

        private bool _isLoading;
        private IReadOnlyList<ServerViewModel>? _servers;
    }
}
