using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Serilog;

namespace HomeLabManager.Manager.Pages;

/// <summary>
/// Base class for page view models.
/// </summary>
public abstract class PageBaseViewModel : ReactiveObject, IDisposable
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

/// <summary>
/// Base class for primary navigation pages with logging support.
/// </summary>
public abstract class PageBaseViewModel<T> : PageBaseViewModel where T : class
{
    /// <summary>
    /// Constructor, set up the logger.
    /// </summary>
    protected PageBaseViewModel() => LogManager = Program.ServiceProvider.Services.GetService<ILogManager>().CreateContextualizedLogManager<T>();

    /// <summary>
    /// Reference to a Longer for pages to use.
    /// </summary>
    protected ContextAwareLogManager<T> LogManager { get; }
}
