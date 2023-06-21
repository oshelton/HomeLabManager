using HomeLabManager.Manager;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Constraints;

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

    [Test]
    public async Task CreateCreateNewServerHost()
    {
        var serverListing = new ServerListingViewModel();

        await serverListing.CreateNewServerHost().ConfigureAwait(false);

        var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

        Assert.That(navigationService.CurrentPage, Is.TypeOf<CreateEditServerViewModel>());

        var createEditPage = navigationService.CurrentPage as CreateEditServerViewModel;

        Assert.That(createEditPage.Title, Is.EqualTo("Create New Server Host"));
    }
}
