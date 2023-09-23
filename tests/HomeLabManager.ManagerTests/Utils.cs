using HomeLabManager.Manager;
using Moq;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.SharedDialogs;
using HomeLabManager.Common.Data.Git;

namespace HomeLabManager.ManagerTests;

/// <summary>
/// Core test utilities.
/// </summary>
internal static class Utils
{
    internal record MockServices(
        Mock<ICoreConfigurationManager> MockCoreConfigurationManager,
        Mock<IServerDataManager> MockServerDataManager,
        Mock<IGitDataManager> MockGitDataManager,
        Mock<INavigationService> MockNavigationService,
        Mock<ISharedDialogsService> MockSharedDialogServices
    );

    /// <summary>
    /// Register applicable mock services and return them.
    /// </summary>
    /// <param name="overrideCoreConfigurationManager">Whether or not the CoreConfigurationManager should be mocked.</param>
    /// <param name="overrideServerDataManager">Whether or not the ServerDataManager should be mocked.</param>
    /// <param name="overrideNavigationService">Whether or not the NavigationService should be mocked.</param>
    /// <remarks>This will need updating as more services are created.</remarks>
    public static MockServices RegisterTestServices(
        bool overrideCoreConfigurationManager = true, 
        bool overrideServerDataManager = true,
        bool overrideGitDataManager = true,
        bool overrideNavigationService = true,
        bool overrideSharedDialogsService = true
    ) {
        var mockCoreConfiguration = overrideCoreConfigurationManager ? new Mock<ICoreConfigurationManager>() : null;
        var mockServerDataManager = overrideServerDataManager ? new Mock<IServerDataManager>() : null;
        var mockGitDataManager = overrideGitDataManager ? new Mock<IGitDataManager>() : null;
        var mockNavigationService = overrideNavigationService ? new Mock<INavigationService>() : null;
        var mockSharedDialogsService = overrideSharedDialogsService ? new Mock<ISharedDialogsService>() : null;

        mockCoreConfiguration?.InitializeMinimumRequiredConfiguration();
        mockServerDataManager?.InitializeMinimumRequiredConfiguration();
        mockGitDataManager?.InitializeMinimumRequiredConfiguration();
        mockNavigationService?.InitializeMinimumRequiredConfiguration();
        mockSharedDialogsService?.InitializeMinimumRequiredConfiguration();

        Program.BuildTestApp(new ServiceOverrides
        {
            CoreConfigurationManagerServiceBuilder = mockCoreConfiguration is not null ? () => mockCoreConfiguration.Object : null,
            ServerDataManagerServiceBuilder = mockServerDataManager is not null ? () => mockServerDataManager.Object : null,
            GitDataManagerServiceBuilder = mockGitDataManager is not null ? () => mockGitDataManager.Object : null,
            NavigationServiceBuilder = mockNavigationService is not null ? () => mockNavigationService.Object : null,
            SharedDialogsServiceBuilder = mockSharedDialogsService is not null ? () => mockSharedDialogsService.Object : null,
        });

        return new MockServices(
            mockCoreConfiguration,
            mockServerDataManager,
            mockGitDataManager,
            mockNavigationService,
            mockSharedDialogsService
        );
    }
}
