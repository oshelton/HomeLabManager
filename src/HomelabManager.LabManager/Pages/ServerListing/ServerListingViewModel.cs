using Avalonia.Interactivity;
using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.ServerListing
{
    /// <summary>
    /// Server Listing Page View Model.
    /// </summary>
    public sealed class ServerListingViewModel : PageBaseViewModel
    {
        public ServerListingViewModel()
        {
            if (Avalonia.Controls.Design.IsDesignMode)
            {
                
            }
        }

        public override string Title => "Server Listing";

        public override async Task NavigateTo(INavigationRequest request)
        {
            if (request is not HomeNavigationRequest)
                throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

            
        }

        public override Task<bool> TryNavigateAway() => Task.FromResult(true);
    }
}
