using HomeLabManager.Manager.Pages.Settings;

namespace HomeLabManager.Manager.Services.Navigation.Requests
{
    /// <summary>
    /// Navigation Request class for the Home Page.
    /// </summary>
    public sealed class SettingsNavigationRequest : INavigationRequest
    {
        /// <summary>
        /// Reference to the Home Page View Model type.
        /// </summary>
        public Type DestinationPageType => typeof(SettingsViewModel);
    }
}
