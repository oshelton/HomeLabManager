﻿using System.Runtime.CompilerServices;
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
    public Task<bool> NavigateTo(INavigationRequest request, PageBaseViewModel? navigateBackToPage = null)
    {
        return Task.FromResult(false);
    }

    public async Task NavigateBack()
    {
        if (!CanNavigateBack)
            return;

        await NavigateTo(_navigationStack[^2].Request, _navigationStack[^2].Page).ConfigureAwait(false);
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
    /// Update whether or not back navigation is possible.
    /// </summary>
    private void UpdateCanNavigateBack() => CanNavigateBack = _navigationStack.Count > 1;

    private readonly List<(INavigationRequest Request, PageBaseViewModel Page)> _navigationStack = new();
}
