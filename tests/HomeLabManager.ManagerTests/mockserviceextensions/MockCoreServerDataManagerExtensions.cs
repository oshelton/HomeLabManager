using HomeLabManager.Common.Data.Git.Server;
using Moq;

namespace HomeLabManager.ManagerTests.MockServiceExtensions;

/// <summary>
/// Class for commonly used mocking helpers for the Server Data Manager Service.
/// </summary>
internal static class MockCoreServerDataManagerExtensions
{
    /// <summary>
    /// Initiallize an IServerDataManager mock with the bare required functionality to get a simple test to run.
    /// </summary>
    public static void InitializeMinimumRequiredConfiguration(this Mock<IServerDataManager> mockService)
    {
        if (mockService is null)
            throw new ArgumentNullException(nameof(mockService));

        mockService.Reset();
    }

    /// <summary>
    /// Setup GetServers to return a configurable number of server hosts.
    /// </summary>
    /// <param name="mockService">Mock service to set up.</param>
    /// <param name="numberServers">Number of servers to create.</param>
    /// <returns>Read only list of created servers.</returns>
    public static IReadOnlyList<ServerHostDto> SetupSimpleServers(this Mock<IServerDataManager> mockService, int numberServers)
    {
        var servers = new List<ServerHostDto>();
        for (var i = 0; i < numberServers; i++)
        {
            servers.Add(new ServerHostDto
                {
                    Metadata = new ServerMetadataDto
                    {
                        DisplayName = $"Test {i + 1}",
                        Name = $"HOST-{i + 1}"
                    },
                    DockerCompose = new DockerComposeDto(),
                    Configuration = new ServerConfigurationDto()
                }
            );
        }

        mockService.Setup(x => x.GetServers())
            .Returns(servers);

        return servers;
    }
}
