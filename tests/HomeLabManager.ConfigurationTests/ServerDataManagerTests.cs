using HomeLabManager.Common.Data.Git;

namespace HomeLabManager.DataTests;

public sealed class ServerDataManagerTests
{
    [SetUp]
    public void SetUp() 
    { 
        Directory.CreateDirectory(Utils.TestDirectory);
        
        Directory.CreateDirectory(Utils.TestGitDirectory);

        var serializer = Common.Data.Utils.CreateBasicYamlSerializer();
        foreach (var server in _servers)
        {
            var directoryName = server.UniqueId?.ToString("D")!;
            Directory.CreateDirectory(Path.Combine(Utils.TestGitDirectory, ServerDataManager.ServersDirectoryName, directoryName));
            File.WriteAllText(Path.Combine(Utils.TestGitDirectory, ServerDataManager.ServersDirectoryName, directoryName, ServerDataManager.ServerMetadataFileName), serializer.Serialize(server.Metadata!));
        }

        File.WriteAllText(Utils.TestGitConfigFilePath, @"[user]\n\tname = Owen Shelton\n\temail = jowenshelton@gmail.com");
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(Utils.TestDirectory, true);
        Directory.Delete(Utils.TestGitDirectory, true);
    }

    /// <summary>
    /// Test retrieving server metadata.
    /// </summary>
    [Test]
    public void GetServerMetadata()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager);

        var servers = serverManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_servers.Length));

        foreach (var retrievedServer in servers)
        {
            var testServer = _servers.First(x => x.UniqueId == retrievedServer.UniqueId);

            Assert.That(retrievedServer.Directory, Is.EqualTo(Path.Combine(Utils.TestGitDirectory, ServerDataManager.ServersDirectoryName, testServer.UniqueId!.ToString()!)));
            Assert.That(retrievedServer.Metadata?.FileName, Is.EqualTo(Path.Combine(Utils.TestGitDirectory, ServerDataManager.ServersDirectoryName, testServer.UniqueId!.ToString()!, ServerDataManager.ServerMetadataFileName)));
            Assert.That(retrievedServer.Metadata?.DisplayName, Is.EqualTo(testServer.Metadata?.DisplayName));
            Assert.That(retrievedServer.Metadata?.Name, Is.EqualTo(testServer.Metadata?.Name));
            Assert.That(retrievedServer.Metadata?.IPAddress, Is.EqualTo(testServer.Metadata?.IPAddress));
            Assert.That(retrievedServer.Metadata?.Description, Is.EqualTo(testServer.Metadata?.Description));
            Assert.That(retrievedServer.Metadata?.Kind, Is.EqualTo(testServer.Metadata?.Kind));
        }
    }

    /// <summary>
    /// Test adding a new server.
    /// </summary>
    [Test]
    public void AddNewServer()
    {
        var testNewServer = new ServerDto()
        {
            Metadata = new ServerMetadataDto()
            {
                DisplayName = "server 4",
                Name = "server4",
                IPAddress = "192.168.1.5",
                Description = "Server 4 Description",
                Kind = ServerKind.Ubuntu,
            },
            DockerCompose = null, //TODO: Fill in when docker compose support is added.
            Configuration = null, //TODO: FIll in when server configuration support is added.
        };

        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager);
        serverManager.AddNewServer(testNewServer);
        Assert.That(testNewServer.UniqueId, Is.Not.Null);

        var servers = serverManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_servers.Length + 1));

        var newlyAddedServer = servers.FirstOrDefault(x => x.UniqueId == testNewServer.UniqueId);
        Assert.That(newlyAddedServer, Is.Not.Null);

        Assert.That(newlyAddedServer.Metadata?.DisplayName, Is.EqualTo(testNewServer.Metadata?.DisplayName));
        Assert.That(newlyAddedServer.Metadata?.Name, Is.EqualTo(testNewServer.Metadata?.Name));
        Assert.That(newlyAddedServer.Metadata?.IPAddress, Is.EqualTo(testNewServer.Metadata?.IPAddress));
        Assert.That(newlyAddedServer.Metadata?.Description, Is.EqualTo(testNewServer.Metadata?.Description));
        Assert.That(newlyAddedServer.Metadata?.Kind, Is.EqualTo(testNewServer.Metadata?.Kind));

        //TODO: Fill in when docker compose support is added.
        //TODO: FIll in when server configuration support is added.
    }

    [Test]
    public void UpdateExistingServer()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager);

        var servers = serverManager.GetServers();

        var toUpdate = servers[0];
        toUpdate = toUpdate with 
        { 
            Metadata = toUpdate.Metadata! with { Name = "Updated Server Name" }
        };

        serverManager.UpdateServer(toUpdate);

        servers = serverManager.GetServers();

        var matchingServer = servers.First(x => x.UniqueId == toUpdate.UniqueId);

        Assert.That(matchingServer.Metadata!.Name, Is.EqualTo(toUpdate.Metadata.Name));

        //TODO: Fill in when docker compose support is added.
        //TODO: FIll in when server configuration support is added.
    }

    [Test]
    public void DeleteServer()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager);

        var servers = serverManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_servers.Length));

        var toDelete = servers[0];

        serverManager.DeleteServer(toDelete);

        servers = serverManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_servers.Length - 1));
        Assert.That(servers.All(x => x.UniqueId != toDelete.UniqueId), Is.True);
    }

    /// <summary>
    /// Testing server dtos.
    /// </summary>
    private static readonly ServerDto[] _servers = new[]
    {
        new ServerDto()
        {
            UniqueId = Guid.NewGuid(),
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
            UniqueId = Guid.NewGuid(),
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
            UniqueId = Guid.NewGuid(),
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
