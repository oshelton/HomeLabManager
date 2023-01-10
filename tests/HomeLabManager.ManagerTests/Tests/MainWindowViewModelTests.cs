using HomeLabManager.Manager;
using HomeLabManager.Manager.Pages.Home;

namespace HomeLabManager.ManagerTests.Tests;

/// <summary>
/// Tests for the main window view model.
/// </summary>
public class MainWindowViewModelTests
{
    [SetUp]
    public void SetUp() => Utils.RegisterTestServices();

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
        var mainWindow = new MainWindowViewModel();

        await mainWindow.WindowLoaded().ConfigureAwait(true);

        Assert.That(mainWindow.NavigationService.CurrentPage, Is.Not.Null);
        Assert.That(mainWindow.NavigationService.CurrentPage.GetType(), Is.EqualTo(typeof(HomeViewModel)));
    }
}
