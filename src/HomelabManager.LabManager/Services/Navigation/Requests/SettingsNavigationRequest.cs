using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.Settings;

namespace HomeLabManager.Manager.Services.Navigation.Requests
{
    /// <summary>
    /// Navigation Request class for the Settings Page.
    /// </summary>
    public sealed class SettingsNavigationRequest : INavigationRequest
    {
        /// <summary>
        /// Create instance of the Settings Page.
        /// </summary>
        public PageBaseViewModel CreatePage() => new SettingsViewModel();
    }
}
