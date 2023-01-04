using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.Git;

namespace HomeLabManager.Manager.DesignModeServices
{
    internal sealed class DesignServerDataManager : IServerDataManager
    {
        public void AddNewServer(ServerDto server) => m_servers.Add(server);

        public void DeleteServer(ServerDto server) => m_servers.Remove(server);

        public IReadOnlyList<ServerDto> GetServers() => m_servers;

        public void UpdateServer(ServerDto server) { }

        private readonly List<ServerDto> m_servers = new()
        {
            new ServerDto()
            {
                Metadata = new ServerMetadataDto()
                {
                    DisplayName = "server 1",
                    Name = "server1",
                    IPAddress = "192.168.1.1",
                    Description = "Server 1 Description",
                    Kind = ServerKind.WindowsWSL,
                },
            },
            new ServerDto()
            {
                Metadata = new ServerMetadataDto()
                {
                    DisplayName = "server 2",
                    Name = "server2",
                    IPAddress = "192.168.1.2",
                    Description = "Server 2 Description",
                    Kind = ServerKind.Ubuntu,
                },
            },
            new ServerDto()
            {
                Metadata = new ServerMetadataDto()
                {
                    DisplayName = "server 3",
                    Name = "server3",
                    IPAddress = "192.168.1.3",
                    Description = "Server 3 Description",
                    Kind = ServerKind.WindowsWSL,
                },
            },
        };
    }
}
