using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.ServerListing;

namespace HomeLabManager.Manager.Services.Navigation.Requests
{
    /// <summary>
    /// Navigation Request class for the Home Page.
    /// </summary>
    public sealed class ServerListingNavigationRequest : INavigationRequest
    {
        /// <summary>
        /// Create instance of the Server Listing Page.
        /// </summary>
        public PageBaseViewModel CreatePage() => new ServerListingViewModel();
    }
}
