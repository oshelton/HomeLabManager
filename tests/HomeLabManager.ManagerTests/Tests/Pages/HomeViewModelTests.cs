using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.ManagerTests.Tests.Pages;

/// <summary>
/// Tests for the Home Page view model.
/// </summary>
public class HomeViewModelTests
{
    [SetUp]
    public void SetUp() => Utils.RegisterTestServices();

    /// <summary>
    /// Test view model creation and initial state.
    /// </summary>
    [Test]
    public void TestCreation()
    {
        var homePage = new HomeViewModel();

        Assert.That(homePage.Title, Is.EqualTo("Home"));
        Assert.That(homePage.CurrentDisplayMode, Is.EqualTo(HomeDisplayMode.NoRepoPath));
        Assert.That(homePage.Servers, Is.Null);
    }

    /// <summary>
    /// Test the logic executed when the page is navigated to.
    /// </summary>
    [Test]
    public async Task TestNavigatingTo()
    {
        var homePage = new HomeViewModel();

        var navigateTask = homePage.NavigateTo(new HomeNavigationRequest());

        await Task.Delay(1000).ConfigureAwait(true);

        Assert.That(homePage.CurrentDisplayMode, Is.EqualTo(HomeDisplayMode.IsLoading));

        await navigateTask.ConfigureAwait(true);

        Assert.That(homePage.CurrentDisplayMode, Is.EqualTo(HomeDisplayMode.HasServers));
        Assert.That(homePage.Servers!, Has.Count.EqualTo(3));
    }
}
