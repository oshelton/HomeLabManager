using HomeLabManager.Manager.Services.Navigation.Requests;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages
{
    /// <summary>
    /// Base class for primary navigation pages.
    /// </summary>
    public abstract class PageBaseViewModel: ReactiveObject
    {
        /// Title of this page.
        public abstract string Title { get; }

        /// <summary>
        /// Navigate to this page.
        /// </summary>
        public abstract Task NavigateTo(INavigationRequest request);

        /// <summary>
        /// Attempt to navigate away from this page.
        /// </summary>
        /// <returns>True if the page deactivated and can be navigated away from, false otherwise.</returns>
        public abstract Task<bool> TryNavigateAway();
    }
}
