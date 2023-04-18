using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.Home;

namespace HomeLabManager.Manager.Services.Navigation.Requests
{
    /// <summary>
    /// Navigation Request class for the Home Page.
    /// </summary>
    public sealed class HomeNavigationRequest : INavigationRequest
    {
        /// <summary>
        /// Create instance of the Home Page.
        /// </summary>
        public PageBaseViewModel CreatePage() => new HomeViewModel();
    }
}
