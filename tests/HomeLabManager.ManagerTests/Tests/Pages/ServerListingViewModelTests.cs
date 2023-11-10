using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager;
using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReactiveUI;
using static HomeLabManager.ManagerTests.Utils;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class ServerListingViewModelTests
{
    [SetUp]
    public void SetUp() => _services = RegisterTestServices();

    /// <summary>
    /// Test view model creation and initial state.
    /// </summary>
    [Test]
    public void Constructor_TestDefaultConstructionBehavior()
    {
        // ARRANGE & ACT
        using (var serverListing = new ServerListingViewModel())
        {
            //ASSERT
            Assert.That(serverListing.Title, Is.EqualTo("Server Listing"));
            Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));
            Assert.That(serverListing.SortedServers, Is.Empty);
        }
    }

    /// <summary>
    /// Test the logic executed when the page is navigated to.
    /// </summary>
    [Test]
    public async Task NavigatingTo_TestNavigatingToTheServerListingPage()
    {
        // ARRANGE
        var servers = _services.MockServerDataManager.SetupSimpleServers(3, generateIds: true);

        using (var serverListing = new ServerListingViewModel())
        {
            // ACT
            var navigateTask = serverListing.NavigateTo(new ServerListingNavigationRequest());

            // ASSERT
            Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));

            await navigateTask.ConfigureAwait(true);

            Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.HasServers));
            Assert.That(serverListing.SortedServers, Has.Count.EqualTo(servers.Count));
        }
    }

    [Test]
    public async Task CreateNewServerHostCommand_NoAfterIndexProvided_ConfirmCreateNewServerHostWithNoServersBehavior()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(1);

            // ACT
            await serverListing.CreateNewServerHostCommand.Execute();

            var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

            // ASSERT
            _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
                It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.IsNew && x.AfterIndex == null), false
            ), Times.Once);
        }
    }

    [Test]
    public async Task CreateNewServerHostCommand_AfterIndexProvided_ConfirmCreateNewServerHostAfterAnotherServerBehavior()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(1);

            var afterIndex = 2;

            // ACT
            await serverListing.CreateNewServerHostCommand.Execute(afterIndex);

            var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

            // ASSERT
            _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
                It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.IsNew && x.AfterIndex == afterIndex), false
            ), Times.Once);
        }
    }

    [Test]
    public async Task EditServerCommand_ValidServerPassed_ConfirmEditServerHostBehavior()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(1);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toEdit = serverListing.SortedServers[0];

            // ACT
            await serverListing.EditServerCommand.Execute(toEdit);

            var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

            // ASSERT
            _services.MockNavigationService.Verify(expression: x => x.NavigateTo(
                It.Is<CreateEditServerNavigationRequest>(x => x.Server is ServerHostDto && x.Server.Metadata.DisplayName == toEdit.DisplayName && !x.IsNew && x.AfterIndex == null), false
            ), Times.Once);
        }
    }

    [Test]
    public async Task MoveServerHostUpCommand_ValidServerPassed_ConfirmMoveServerHostUpBehavior()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(5, generateIds: true);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toMoveUp = serverListing.SortedServers[1];

            // ACT
            await serverListing.MoveServerUpCommand.Execute(toMoveUp);

            // ASSERT
            Assert.That(toMoveUp.DisplayIndex, Is.EqualTo(0));

            // Need a delay here for sorted servers to update.
            await Task.Delay(50).ConfigureAwait(true);

            Assert.That(serverListing.SortedServers[1].DisplayIndex, Is.EqualTo(1));

            _services.MockServerDataManager.Verify(x => x.AddUpdateServer(It.IsAny<ServerHostDto>()), Times.Exactly(2));
        }
    }

    [Test]
    public async Task MoveServerHostUpCommand_InvalidServerPassed_ConfirmMoveServerHostUpThrowsExceptionWhenInvalidPassed()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(5, generateIds: true);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toMoveUp = serverListing.SortedServers[0];
            
            // ACT & ASSERT
            Assert.ThrowsAsync<UnhandledErrorException>(() => serverListing.MoveServerUpCommand.Execute(toMoveUp).ToTask());

            _services.MockServerDataManager.Verify(x => x.AddUpdateServer(It.IsAny<ServerHostDto>()), Times.Never());
        }
    }

    [Test]
    public async Task MoveServerHostDownCommand_ValidServerPassed_ConfirmMoveServerHostDownBehavior()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(5, generateIds: true);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toMoveDown = serverListing.SortedServers[1];

            // ACT
            await serverListing.MoveServerDownCommand.Execute(toMoveDown);

            // ASSERT
            Assert.That(toMoveDown.DisplayIndex, Is.EqualTo(2));

            // Need a delay here for sorted servers to update.
            await Task.Delay(50).ConfigureAwait(true);

            Assert.That(serverListing.SortedServers[1].DisplayIndex, Is.EqualTo(1));

            _services.MockServerDataManager.Verify(x => x.AddUpdateServer(It.IsAny<ServerHostDto>()), Times.Exactly(2));
        }
    }

    [Test]
    public async Task MoveServerHostDownCommand_InvalidServerPassed_ConfirmMoveServerHostDownThrowsExceptionWhenInvalidPassed()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(5, generateIds: true);
            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toMoveDown = serverListing.SortedServers.Last();
            
            // ACT & ASSERT
            Assert.ThrowsAsync<UnhandledErrorException>(() => serverListing.MoveServerDownCommand.Execute(toMoveDown).ToTask());

            _services.MockServerDataManager.Verify(x => x.AddUpdateServer(It.IsAny<ServerHostDto>()), Times.Never());
        }
    }

    [Test]
    public async Task DeleteServerCommand_DeleteAServer_ConfirmMoveServerHostDeletionWorksAsExpected()
    {
        // ARRANGE
        using (var serverListing = new ServerListingViewModel())
        {
            _services.MockServerDataManager.SetupSimpleServers(5, generateIds: true);
            _services.MockSharedDialogServices.Setup(x => x.ShowSimpleYesNoDialog(It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            await serverListing.NavigateTo(new ServerListingNavigationRequest()).ConfigureAwait(true);

            var toDelete = serverListing.SortedServers[2];
            var initialCount = serverListing.SortedServers.Count;
            var toDeleteAfterServers = serverListing.SortedServers.Where(x => x.DisplayIndex > toDelete.DisplayIndex);

            // ACT
            await serverListing.DeleteServerCommand.Execute(toDelete);

            // Need a delay here for sorted servers to update.
            await Task.Delay(50).ConfigureAwait(true);

            // ASSERT
            Assert.That(serverListing.SortedServers.Contains(toDelete), Is.False);
            Assert.That(serverListing.SortedServers, Has.Count.EqualTo(initialCount - 1));

            for (int i = toDelete.DisplayIndex; i < serverListing.SortedServers.Count; i++) 
            {
                Assert.That(serverListing.SortedServers[i].DisplayIndex, Is.EqualTo(i));
            }

            _services.MockServerDataManager.Verify(x => x.DeleteServer(It.IsAny<ServerHostDto>()), Times.Once);
        }
    }

    private MockServices _services;
}
