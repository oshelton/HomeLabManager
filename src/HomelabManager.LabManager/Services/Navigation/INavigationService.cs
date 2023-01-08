using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.Manager.Services.Navigation;

/// <summary>
/// Interface for Navigation Service implementation.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate to a page using a NavigationRequest.
    /// </summary>
    /// <returns>false if navigation fails, true if it succeeds.</returns>
    Task<bool> NavigateTo(INavigationRequest request, bool isBackNavigation = false);
    
    /// <summary>
    /// Navigate back to the previous page.
    /// </summary>
    /// <returns>false if navigation fails, true if it succeeds.</returns>
    Task<bool> NavigateBack();
    
    /// <summary>
    /// Get whether or not back navigation is possible.
    /// </summary>
    bool CanNavigateBack { get; }

    /// <summary>
    /// Get the current page.
    /// </summary>
    PageBaseViewModel? CurrentPage { get; }

    /// <summary>
    /// Get a reference to all available pages.
    /// </summary>
    IReadOnlyList<PageBaseViewModel> Pages { get; }
}
