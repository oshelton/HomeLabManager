﻿using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Avalonia.Collections;
using HomeLabManager.Manager;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
        var serverListing = new ServerListingViewModel();

        var navigateTask = serverListing.NavigateTo(new ServerListingNavigationRequest());

        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.IsLoading));

        await navigateTask.ConfigureAwait(true);

        Assert.That(serverListing.CurrentDisplayMode, Is.EqualTo(ServerListingDisplayMode.HasServers));
        Assert.That(serverListing.SortedServers, Has.Count.EqualTo(3));

        serverListing.Dispose();
    }

    [Test]
    public async Task CreateNewServerHost_ConfirmCreateNewServerHostDefaultBehavior()
    {    
        var serverListing = new Mock<ServerListingViewModel>();
        serverListing.SetupGet(x => x.SortedServers).Returns(new ReadOnlyObservableCollection<ServerViewModel>(new ObservableCollection<ServerViewModel>()));

        await serverListing.Object.CreateNewServerHostCommand.Execute();

        var navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

        Assert.That(navigationService.CurrentPage, Is.TypeOf<CreateEditServerViewModel>());

        var createEditPage = navigationService.CurrentPage as CreateEditServerViewModel;

        Assert.That(createEditPage.Title, Is.EqualTo("Create New Server Host"));
    }
}
