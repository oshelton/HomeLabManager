using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using ReactiveUI;

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// Design time Navigation Service.
/// </summary>
internal sealed class DesignNavigationService: ReactiveObject, INavigationService
{
    public DesignNavigationService()
    {
        var homePage = new HomeViewModel();
        CurrentPage = homePage;
        Pages = new[] { homePage };
    }

    public bool CanNavigateBack => true;

    public PageBaseViewModel? CurrentPage { get; }

    public IReadOnlyList<PageBaseViewModel> Pages { get; }

    public Task<bool> NavigateBack() => Task.FromResult(false);

    public Task<bool> NavigateTo(INavigationRequest request, bool isBackNavigation = false) => Task.FromResult(false);
}
