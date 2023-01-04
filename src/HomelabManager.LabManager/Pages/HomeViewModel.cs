using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using HomeLabManager.Common.Data.Git;
using HomeLabManager.Manager.Pages.HomeComponents;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages
{
    public sealed class HomeViewModel : PageBaseViewModel
    {
        public HomeViewModel() 
        {
            _serverDataManager = Program.ServiceProvider!.Services.GetService<IServerDataManager>()!;

            if (Avalonia.Controls.Design.IsDesignMode)
                _servers = _serverDataManager.GetServers().Select(x => new ServerViewModel(x)).ToArray();
        }

        public override string Title => "Home";

        public override async void Activate() 
        {
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

        public override bool TryDeactivate() => true;

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
