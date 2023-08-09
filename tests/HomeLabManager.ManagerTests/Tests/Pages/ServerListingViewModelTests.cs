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
    public async Task NavigatingTo_TestNavigatingToTheServerListingPage()
    {
        var servers = _services.MockServerDataManager.SetupSimpleServers(3, generateIds: true);

        using (var serverListing = new ServerListingViewModel())
        {
            var navigateTask = serverListing.NavigateTo(new ServerListingNavigationRequest());

            Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));

            await navigateTask.ConfigureAwait(true);

            Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.HasServers));
            Assert.That(serverListing.SortedServers, Has.Count.EqualTo(servers.Count));
        }
    }

    [Test]
    public async Task CreateNewServerHostCommand_NoAfterIndexProvided_ConfirmCreateNewServerHostWithNoServersBehavior()
    {
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(1);

            await serverListing.CreateNewServerHostCommand.Execute();

            var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

            _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
                It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.IsNew && x.AfterIndex == null), false
            ), Times.Once);
        }
    }

    [Test]
    public async Task CreateNewServerHostCommand_AfterIndexProvided_ConfirmCreateNewServerHostAfterAnotherServerBehavior()
    {
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(1);

            var afterIndex = 2;
            await serverListing.CreateNewServerHostCommand.Execute(afterIndex);

            var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

            _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
                It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.IsNew && x.AfterIndex == afterIndex), false
            ), Times.Once);
        }
    }

    [Test]
    public async Task EditServerCommand_ValidServerPassed_ConfirmEditServerHostBehavior()
    {
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(1);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toEdit = serverListing.SortedServers[0];
            await serverListing.EditServerCommand.Execute(toEdit);

            var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

            _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
                It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.Server.Metadata.DisplayName == toEdit.DisplayName && !x.IsNew && x.AfterIndex == null), false
            ), Times.Once);
        }
    }

    [Test]
    public async Task MoveServerHostUpCommand_ValidServerPassed_ConfirmMoveServerHostUpBehavior()
    {
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(5, generateIds: true);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toMoveUp = serverListing.SortedServers[1];
            await serverListing.MoveServerUpCommand.Execute(toMoveUp);

            Assert.That(toMoveUp.DisplayIndex, Is.EqualTo(0));

            // Need a delay here for sorted servers to update.
            await Task.Delay(50).ConfigureAwait(true);

            Assert.That(serverListing.SortedServers[1].DisplayIndex, Is.EqualTo(1));

            _services.MockServerDataManager.Verify(x => x.AddUpdateServer(It.IsAny<ServerHostDto>()), Times.Exactly(2));
        }
    }

    private (
        Mock<ICoreConfigurationManager> MockCoreConfigManager,
        Mock<IServerDataManager> MockServerDataManager,
        Mock<INavigationService> MockNavigationService
    ) _services;
}
