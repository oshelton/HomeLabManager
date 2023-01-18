using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.ManagerTests.Tests.Services;

/// <summary>
/// Tests related ot the release NavigationService.
/// </summary>
public class NavigationServiceTests
{
    /// <summary>
    /// Test initial creation and state of the Navigation Service.
    /// </summary>
    [Test]
    public void TestCreation()
    {
        var service = new NavigationService();

        Assert.That(service.CanNavigateBack, Is.False);
        Assert.That(service.Pages, Has.Count.EqualTo(2));
        Assert.That(service.CurrentPage, Is.Null);
    }

    /// <summary>
    /// Test the initial navigation for the service and that it leaves it in the expected state.
    /// </summary>
    [Test]
    public async Task TestInitialNavigation()
    {
        var service = new NavigationService();

        await service.NavigateTo(new HomeNavigationRequest()).ConfigureAwait(true);

        Assert.That(service.CanNavigateBack, Is.False);
        Assert.That(service.Pages, Has.Count.EqualTo(2));
        Assert.That(service.CurrentPage!.GetType(), Is.EqualTo(typeof(HomeViewModel)));
    }
}
