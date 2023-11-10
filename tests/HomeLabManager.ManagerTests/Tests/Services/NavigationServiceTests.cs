using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Pages.Settings;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using static HomeLabManager.ManagerTests.Utils;

namespace HomeLabManager.ManagerTests.Tests.Services;

/// <summary>
/// Tests related to the NavigationService.
/// </summary>
public class NavigationServiceTests
{
    [SetUp]
    public void SetUp() => RegisterTestServices();

    [Test]
    public void NavigationService_Creation_TestDefaultConstructor()
    {
        var service = new NavigationService(new LogManager(true));

        Assert.That(service.CanNavigateBack, Is.False);
        Assert.That(service.CurrentPage, Is.Null);
    }

    [Test]
    public async Task NavigateTo_InitialNavigation_TestInitialNavigationToHomePage()
    {
        var service = new NavigationService(new LogManager(true));

        await service.NavigateTo(new HomeNavigationRequest()).ConfigureAwait(true);

        Assert.That(service.CanNavigateBack, Is.False);
        Assert.That(service.CurrentPage!.GetType(), Is.EqualTo(typeof(HomeViewModel)));
    }

    [Test]
    public async Task NavigateTo_SecondPageNavigation_TestNavigatingToASecondPage()
    {
        var service = new NavigationService(new LogManager(true));

        await service.NavigateTo(new HomeNavigationRequest()).ConfigureAwait(true);
        await service.NavigateTo(new SettingsNavigationRequest()).ConfigureAwait(true);

        Assert.That(service.CanNavigateBack, Is.True);
        Assert.That(service.CurrentPage!.GetType(), Is.EqualTo(typeof(SettingsViewModel)));
    }

    [Test]
    public async Task NavigateBack_TestNavigateBackFunctionality_ConfirmNavigatingBackWorksAsExpected()
    {
        var service = new NavigationService(new LogManager(true));

        await service.NavigateTo(new HomeNavigationRequest()).ConfigureAwait(true);
        await service.NavigateTo(new SettingsNavigationRequest()).ConfigureAwait(true);

        await service.NavigateBack().ConfigureAwait(true);

        Assert.That(service.CanNavigateBack, Is.False);
        Assert.That(service.CurrentPage!.GetType(), Is.EqualTo(typeof(HomeViewModel)));
    }
}
