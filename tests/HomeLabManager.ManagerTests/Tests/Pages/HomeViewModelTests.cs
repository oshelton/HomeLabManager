using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.Home;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Moq;

namespace HomeLabManager.ManagerTests.Tests.Pages;

/// <summary>
/// Tests for the Home Page view model.
/// </summary>
public class HomeViewModelTests
{
    [SetUp]
    public void SetUp() => _services = Utils.RegisterTestServices();

    /// <summary>
    /// Test view model creation and initial state.
    /// </summary>
    [Test]
    public void Constructor_DefaultConstruction_TestDefaultHomePageConstructionExpectations()
    {
        var homePage = new HomeViewModel();

        Assert.That(homePage.Title, Is.EqualTo("Home"));
        Assert.That(homePage.CurrentDisplayMode, Is.EqualTo(HomeDisplayMode.NoRepoPath));
        Assert.That(homePage.Servers, Is.Null);

        homePage.Dispose();
    }

    /// <summary>
    /// Test the logic executed when the page is navigated to.
    /// </summary>
    [Test]
    public async Task NavigateTo_HappyPathNavigateTo_ConfirmNavigatingToExpectationsWithFullConfiguration()
    {
        _services.MockCoreConfigManager.Setup(x => x.GetCoreConfiguration())
            .Returns(new CoreConfigurationDto
            {
                HomeLabRepoDataPath = Path.GetTempPath(),
            });

        _services.MockServerDatamanager.Setup(x => x.GetServers())
            .Returns(new[]
            {
                new ServerHostDto
                {
                    Metadata = new ServerMetadataDto
                    {
                        DisplayName = "Test 1",
                        Name = "HOST-1"
                    }
                },
                new ServerHostDto
                {
                    Metadata = new ServerMetadataDto
                    {
                        DisplayName = "Test 2",
                        Name = "HOST-2"
                    }
                },
                new ServerHostDto
                {
                    Metadata = new ServerMetadataDto
                    {
                        DisplayName = "Test 3",
                        Name = "HOST-3"
                    }
                }
            });

        var homePage = new HomeViewModel();
        var navigateTask = homePage.NavigateTo(new HomeNavigationRequest());

        Assert.That(homePage.CurrentDisplayMode, Is.EqualTo(HomeDisplayMode.IsLoading));

        await navigateTask.ConfigureAwait(true);

        Assert.That(homePage.CurrentDisplayMode, Is.EqualTo(HomeDisplayMode.HasServers));
        Assert.That(homePage.Servers!, Has.Count.EqualTo(3));

        homePage.Dispose();
    }

    private (
        Mock<ICoreConfigurationManager> MockCoreConfigManager, 
        Mock<IServerDataManager> MockServerDatamanager, 
        Mock<INavigationService> MockNavigationService
    ) _services;
}
