using HomeLabManager.Manager.Pages.Home;

namespace HomeLabManager.Manager.Services.Navigation.Requests
{
    /// <summary>
    /// Navigation Request class for the Home Page.
    /// </summary>
    public sealed class HomeNavigationRequest : INavigationRequest
    {
        /// <summary>
        /// Reference to the Home Page View Model type.
        /// </summary>
        public Type DestinationPageType => typeof(HomeViewModel);
    }
}
