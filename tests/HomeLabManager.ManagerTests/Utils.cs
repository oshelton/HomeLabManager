using HomeLabManager.Manager;
using Moq;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;

namespace HomeLabManager.ManagerTests;

/// <summary>
/// Core test utilities.
/// </summary>
internal static class Utils
{
    /// <summary>
    /// Register applicable mock services and return them.
    /// </summary>
    /// <param name="overrideCoreConfigurationManager">Whether or not the CoreConfigurationManager should be mocked.</param>
    /// <param name="overrideServerDataManager">Whether or not the ServerDataManager should be mocked.</param>
    /// <param name="overrideNavigationService">Whether or not the NavigationService should be mocked.</param>
    /// <remarks>This will need updating as more services are created.</remarks>
    public static (
        Mock<ICoreConfigurationManager> MockCoreConfigurationManager,
        Mock<IServerDataManager> MockCoreServerDataManager,
        Mock<INavigationService> MockNavigationService
    ) RegisterTestServices(bool overrideCoreConfigurationManager = true, bool overrideServerDataManager = true, bool overrideNavigationService = true)
    {
        var mockCoreConfiguration = overrideCoreConfigurationManager ? new Mock<ICoreConfigurationManager>() : null;
        var mockServerDataManager = overrideServerDataManager ? new Mock<IServerDataManager>() : null;
        var mockNavigationService = overrideNavigationService ? new Mock<INavigationService>() : null;

        mockCoreConfiguration?.InitializeMinimumRequiredConfiguration();
        mockServerDataManager?.InitializeMinimumRequiredConfiguration();
        mockNavigationService?.InitializeMinimumRequiredConfiguration();

        Program.BuildTestApp(new ServiceOverrides
        {
            CoreConfigurationManagerServiceBuilder = mockCoreConfiguration is not null ? () => mockCoreConfiguration.Object : null,
            ServerDataManagerServiceBuilder = mockServerDataManager is not null ? () => mockServerDataManager.Object : null,
            NavigationServiceBuilder = mockNavigationService is not null ? () => mockNavigationService.Object : null,
        });

        return (
            mockCoreConfiguration,
            mockServerDataManager,
            mockNavigationService
        );
    }
}
