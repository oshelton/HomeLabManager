using System.Collections.ObjectModel;
using System.Reactive.Linq;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager;
using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class ServerListingViewModelTests
{
    [SetUp]
    public void SetUp() => _services = Utils.RegisterTestServices();

    /// <summary>
    /// Test view model creation and initial state.
    /// </summary>
    [Test]
    public void Constructor_TestDefaultConstructionBehavior()
    {
        var serverListing = new ServerListingViewModel();

        Assert.That(serverListing.Title, Is.EqualTo("Server Listing"));
        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));
        Assert.That(serverListing.SortedServers, Is.Empty);

        serverListing.Dispose();
    }

    /// <summary>
    /// Test the logic executed when the page is navigated to.
    /// </summary>
    [Test]
    [Ignore("Until we get a mocked server data manager.")]
    public async Task NavigatingTo_TestNavigatingToTheServerListingPage()
    {
        var servers = _services.MockServerDataManager.SetupSimpleServers(3, generateIds: true);

        var serverListing = new ServerListingViewModel();

        var navigateTask = serverListing.NavigateTo(new ServerListingNavigationRequest());

        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));

        await navigateTask.ConfigureAwait(true);

        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.HasServers));
        Assert.That(serverListing.SortedServers, Has.Count.EqualTo(servers.Count));

        serverListing.Dispose();
    }

    [Test]
    public async Task CreateNewServerHost_ConfirmCreateNewServerHostDefaultBehavior()
    {    
        var serverListing = new Mock<ServerListingViewModel>();
        serverListing.SetupGet(x => x.SortedServers).Returns(new ReadOnlyObservableCollection<ServerViewModel>(new ObservableCollection<ServerViewModel>()));

        await serverListing.Object.CreateNewServerHostCommand.Execute();

        var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

        _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
            It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.IsNew && x.AfterIndex == null), false
        ), Times.Once);
    }

    private (
        Mock<ICoreConfigurationManager> MockCoreConfigManager,
        Mock<IServerDataManager> MockServerDataManager,
        Mock<INavigationService> MockNavigationService
    ) _services;
}
