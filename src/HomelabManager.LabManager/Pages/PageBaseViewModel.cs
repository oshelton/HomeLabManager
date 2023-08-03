using HomeLabManager.Manager.Services.Navigation.Requests;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages
{
    /// <summary>
    /// Base class for primary navigation pages.
    /// </summary>
    public abstract class PageBaseViewModel: ReactiveObject, IDisposable
    {
        /// Title of this page.
        public abstract string Title { get; }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Navigate to this page.
        /// </summary>
        public abstract Task NavigateTo(INavigationRequest request);

        /// <summary>
        /// Attempt to navigate away from this page.
        /// </summary>
        /// <returns>True if the page deactivated and can be navigated away from, false otherwise.</returns>
        public abstract Task<bool> TryNavigateAway();

        /// <summary>
        /// Method for cleaning up a page once it has been navigated away from.
        /// </summary>
        /// <param name="isDisposing"></param>
        protected abstract void Dispose(bool isDisposing);
    }
}
