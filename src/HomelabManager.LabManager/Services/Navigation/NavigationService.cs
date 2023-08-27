using Avalonia.Threading;
using HomeLabManager.Common.Services;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using ReactiveUI;
using Serilog;

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
        _logger = logManager?.ApplicationLogger.ForContext<NavigationService>() ?? throw new ArgumentNullException(nameof(logManager));

    /// <summary>
    /// Navigate to a different page.
    /// </summary>
    public async Task<bool> NavigateTo(INavigationRequest request, bool isBackNavigation = false)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var destinationPage = request.CreatePage();

        if (destinationPage is null)
            throw new InvalidOperationException("INavigationRequest must have a destination page.");

        if (CurrentPage is not null)
        {
            var result = await CurrentPage.TryNavigateAway().ConfigureAwait(false);
            if (!result)
                return false;
            else
                CurrentPage.Dispose();
        }

        await destinationPage.NavigateTo(request).ConfigureAwait(false);

        await DispatcherHelper.InvokeAsync(() =>
        {
            CurrentPage = destinationPage;

            if (!isBackNavigation)
                _navigationStack.Add(request);
            else
                _navigationStack.RemoveAt(_navigationStack.Count - 1);
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
            return Task.CompletedTask;

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
    private ILogger _logger;

    private bool _canNavigateBack;
    private PageBaseViewModel _currentPage;
}
