using System.Runtime.CompilerServices;
using Avalonia.Threading;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Pages.Settings;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using ReactiveUI;

[assembly: InternalsVisibleTo("HomeLabManager.ManagerTests")]

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// Design time Navigation Service.
/// </summary>
internal sealed class DesignNavigationService: ReactiveObject, INavigationService
{
    public DesignNavigationService()
    {
        Pages = new PageBaseViewModel[]
        {
                new HomeViewModel(),
                new SettingsViewModel(),
        };
    }

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

    public async Task NavigateBack()
    {
        if (!CanNavigateBack)
            return;

        await NavigateTo(_navigationStack[^2], true).ConfigureAwait(false);
    }

    /// <summary>
    /// Get whether or not navigating back is possible.
    /// </summary>
    public bool CanNavigateBack { get; private set; }

    /// <summary>
    /// Get the current page.
    /// </summary>
    public PageBaseViewModel? CurrentPage { get; private set; }

    /// <summary>
    /// Get all available pages.
    /// </summary>
    public IReadOnlyList<PageBaseViewModel> Pages { get; private set; }

    /// <summary>
    /// Update whether or not back navigation is possible.
    /// </summary>
    private void UpdateCanNavigateBack() => CanNavigateBack = _navigationStack.Count > 1;

    private List<INavigationRequest> _navigationStack = new();
}
