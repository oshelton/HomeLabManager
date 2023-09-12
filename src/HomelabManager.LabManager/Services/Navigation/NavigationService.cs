using Avalonia.Threading;
using HomeLabManager.Common.Services;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using ReactiveUI;

namespace HomeLabManager.Manager.Services.Navigation;

/// <summary>
/// Runtime class for handling Navigation between pages.
/// </summary>
public sealed class NavigationService: ReactiveObject, INavigationService
{
    /// <summary>
    /// Constructor, sets up a logger.
    /// </summary>
    public NavigationService(ILogManager logManager) =>
        _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));

    /// <summary>
    /// Navigate to a different page.
    /// </summary>
    public async Task<bool> NavigateTo(INavigationRequest request, bool isBackNavigation = false)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var logger = _logManager.GetApplicationLoggerForContext<NavigationService>();
        logger.Information("Exictuing NavigateTo for request of Type \"{Type}\" and is back navigation \"{IsBack}\"", request.GetType().Name, isBackNavigation);

        logger.Verbose("Creating page for navigation request.");
        var destinationPage = request.CreatePage() ?? throw new InvalidOperationException("INavigationRequest must have a destination page.");

        if (CurrentPage is not null)
        {
            logger.Verbose("Attempting to navigate away from current page.");
            var result = await CurrentPage.TryNavigateAway().ConfigureAwait(false);
            if (!result)
            {
                logger.Information("Navigation aborted by current page of type \"{CurrentPageType}\".", CurrentPage.GetType().Name);
                return false;
            }
            else
            {
                CurrentPage.Dispose();
            }
        }

        logger.Verbose("Navigating to new page");
        await destinationPage.NavigateTo(request).ConfigureAwait(false);

        await DispatcherHelper.InvokeAsync(() =>
        {
            CurrentPage = destinationPage;

            if (!isBackNavigation)
            {
                logger.Verbose("Adding previous page's request to navigation stack");
                _navigationStack.Add(request);
            }
            else
            {
                logger.Verbose("Removing previous page's request from navigation stack");
                _navigationStack.RemoveAt(_navigationStack.Count - 1);
            }
            UpdateCanNavigateBack();
        }, DispatcherPriority.Input).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Navigate back to the previous page.
    /// </summary>
    public Task NavigateBack()
    {
        if (!CanNavigateBack)
        {
            _logManager.GetApplicationLoggerForContext<NavigationService>().Warning("Cannot navigate back, request aborted");
            return Task.CompletedTask;
        }

        return NavigateTo(_navigationStack[^2], true);
    }

    /// <summary>
    /// Get whether or not navigating back is possible.
    /// </summary>
    public bool CanNavigateBack
    {
        get => _canNavigateBack;
        private set => this.RaiseAndSetIfChanged(ref _canNavigateBack, value);
    }

    /// <summary>
    /// Get the current page.
    /// </summary>
    public PageBaseViewModel CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    /// <summary>
    /// Update whether or not back navigation is possible.
    /// </summary>
    private void UpdateCanNavigateBack() => CanNavigateBack = _navigationStack.Count > 1;

    private readonly List<INavigationRequest> _navigationStack = new();
    private readonly ILogManager _logManager;

    private bool _canNavigateBack;
    private PageBaseViewModel _currentPage;
}
