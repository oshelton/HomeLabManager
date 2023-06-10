using System.Runtime.CompilerServices;
using HomeLabManager.Common.Data.Git.Server;

[assembly: InternalsVisibleTo("HomeLabManager.ManagerTests")]

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// Test time Server Data Manager implementation.
/// </summary>
internal sealed class TestServerDataManager: IServerDataManager
{
    public void AddUpdateServer(ServerHostDto server) => m_servers.Add(server);

    public void DeleteServer(ServerHostDto server) => m_servers.Remove(server);

    public IReadOnlyList<ServerHostDto> GetServers()
    {
        Thread.Sleep(10000);

        return m_servers;
    }

    public void UpdateServer(ServerHostDto server) { }

    private readonly List<ServerHostDto> m_servers = new()
    {
        new ServerHostDto()
        {
            Metadata = new ServerMetadataDto()
            {
                DisplayName = "server 1",
                Name = "server1",
                IPAddress = "192.168.1.1",
                Description = "Server 1 Description",
                Kind = ServerKind.Windows,
            },
        },
        new ServerHostDto()
        {
            Metadata = new ServerMetadataDto()
            {
                DisplayName = "server 2",
                Name = "server2",
                IPAddress = "192.168.1.2",
                Description = "Server 2 Description",
                Kind = ServerKind.StandardLinux,
            },
        },
        new ServerHostDto()
        {
            Metadata = new ServerMetadataDto()
            {
                DisplayName = "server 3",
                Name = "server3",
                IPAddress = "192.168.1.3",
                Description = "Server 3 Description",
                Kind = ServerKind.Windows,
            },
            VMs = new[]
            {
                new ServerVmDto()
                {
                    Metadata = new ServerMetadataDto()
                    {
                        DisplayName = "VM 1",
                        Name = "vm1",
                        IPAddress = "192.168.1.4",
                        Description = "VM 1 Description",
                        Kind = ServerKind.StandardLinux,
                    },
                },
                new ServerVmDto()
                {
                    Metadata = new ServerMetadataDto()
                    {
                        DisplayName = "VM 2",
                        Name = "vm2",
                        IPAddress = "192.168.1.5",
                        Description = "VM 2 Description",
                        Kind = ServerKind.StandardLinux,
                    },
                },
            }
        },
    };
}
