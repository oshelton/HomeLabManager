using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using ReactiveUI;

namespace HomeLabManager.Manager.Services.Navigation;

/// <summary>
/// Runtime class for handling Navigation between pages.
/// </summary>
public sealed class NavigationService: ReactiveObject, INavigationService
{
    public NavigationService() 
    {
        Pages = new[]
        {
                new HomeViewModel(),
        };
    }

    /// <summary>
    /// Navigate to a different page.
    /// </summary>
    public async Task<bool> NavigateTo(INavigationRequest request, bool isBackNavigation = false)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var destinationPageType = request.DestinationPageType;
        var destinationPage = Pages.FirstOrDefault(x => x.GetType() == destinationPageType);

        if (destinationPage is null)
            throw new InvalidOperationException("INavigationRequest must have a destination page.");

        if (CurrentPage is not null)
        {
            var result = await destinationPage.TryNavigateAway().ConfigureAwait(false);
            if (!result)
                return false;
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
    public async Task<bool> NavigateBack()
    {
        if (!CanNavigateBack)
            return false;

        return await NavigateTo(_navigationStack[^2], true).ConfigureAwait(false);
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
    public PageBaseViewModel? CurrentPage
    {
        get => _currentPage;
        private set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    /// <summary>
    /// Get all available pages.
    /// </summary>
    public IReadOnlyList<PageBaseViewModel> Pages
    {
        get => _pages;
        private set => this.RaiseAndSetIfChanged(ref _pages, value);
    }

    /// <summary>
    /// Update whether or not back navigation is possible.
    /// </summary>
    private void UpdateCanNavigateBack() => CanNavigateBack = _navigationStack.Count > 1;

    private List<INavigationRequest> _navigationStack = new();
    private bool _canNavigateBack;
    private PageBaseViewModel? _currentPage;
    private IReadOnlyList<PageBaseViewModel> _pages = new List<PageBaseViewModel>();
}
