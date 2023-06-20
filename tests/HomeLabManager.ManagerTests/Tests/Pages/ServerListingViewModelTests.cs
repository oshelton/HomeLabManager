using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class ServerListingViewModelTests
{
    [SetUp]
    public void SetUp() => Utils.RegisterTestServices();

    /// <summary>
    /// Test view model creation and initial state.
    /// </summary>
    [Test]
    public void TestCreation()
    {
        var serverListing = new ServerListingViewModel();

        Assert.That(serverListing.Title, Is.EqualTo("Server Listing"));
        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));
        Assert.That(serverListing.Servers, Is.Null);
    }

    /// <summary>
    /// Test the logic executed when the page is navigated to.
    /// </summary>
    [Test]
    public async Task TestNavigatingTo()
    {
        var serverListing = new ServerListingViewModel();

        var navigateTask = serverListing.NavigateTo(new ServerListingNavigationRequest());

        await Task.Delay(1000).ConfigureAwait(true);

        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));

        await navigateTask.ConfigureAwait(true);

        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.HasServers));
        Assert.That(serverListing.Servers, Has.Count.EqualTo(3));
    }
}
