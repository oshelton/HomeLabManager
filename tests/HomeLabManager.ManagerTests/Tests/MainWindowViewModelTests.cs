using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Windows;
using Moq;

namespace HomeLabManager.ManagerTests.Tests;

/// <summary>
/// Tests for the main window view model.
/// </summary>
public class MainWindowViewModelTests
{
    [SetUp]
    public void Setup() => _services = Utils.RegisterTestServices();


    /// <summary>
    /// Test basic Main Window View Model Creation.
    /// </summary>
    [Test]
    public void TestCreation()
    {
        var mainWindow = new MainWindowViewModel();

        Assert.That(mainWindow.NavigationService, Is.Not.Null);
    }

    /// <summary>
    /// Test Creating the Main Window View Model and performing the logic when the window loads.
    /// </summary>
    [Test]
    public async Task TestInitialization()
    {
        var navigateToSetup = _services.MockNavigationService.Setup(x => x.NavigateTo(It.IsAny<HomeNavigationRequest>(), false));

        var mainWindow = new MainWindowViewModel();

        await mainWindow.WindowLoaded().ConfigureAwait(true);

        _services.MockNavigationService.Verify(x => x.NavigateTo(It.IsAny<HomeNavigationRequest>(), false), Times.Once());
    }

    private Utils.MockServices _services;
}
