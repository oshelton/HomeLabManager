using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git;
using LibGit2Sharp;
using YamlDotNet.Serialization;

namespace HomeLabManager.DataTests;

public sealed class GitDataManagerTests
{
    [SetUp]
    public void SetUp() 
    { 
        Directory.CreateDirectory(Utils.TestDirectory);
        
        Directory.CreateDirectory(Utils.TestGitDirectory);

        var serializer = Common.Data.Utils.CreateBasicYamlSerializer();
        foreach (var metadata in _metadataDtos)
        {
            Directory.CreateDirectory(Path.Combine(Utils.TestGitDirectory, GitDataManager.ServersDirectoryName, metadata.Name!));
            File.WriteAllText(Path.Combine(Utils.TestGitDirectory, GitDataManager.ServersDirectoryName, metadata.Name!, GitDataManager.ServerMetadataFileName), serializer.Serialize(metadata));
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
        var gitManager = new GitDataManager(Utils.CreateCoreConfigurationManager(false).manager);

        var servers = gitManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_metadataDtos.Length));

        for (int i = 0; i < servers.Count; i++)
        {
            var testMetadata = _metadataDtos[i];
            var retrievedServer = servers[i];
             
            Assert.That(retrievedServer.Metadata?.DisplayName, Is.EqualTo(testMetadata.DisplayName));
            Assert.That(retrievedServer.Metadata?.Name, Is.EqualTo(testMetadata.Name));
            Assert.That(retrievedServer.Metadata?.IPAddress, Is.EqualTo(testMetadata.IPAddress));
            Assert.That(retrievedServer.Metadata?.Description, Is.EqualTo(testMetadata.Description));
            Assert.That(retrievedServer.Metadata?.Kind, Is.EqualTo(testMetadata.Kind));
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

        var gitManager = new GitDataManager(Utils.CreateCoreConfigurationManager(false).manager);
        gitManager.AddNewServer(testNewServer);

        var servers = gitManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_metadataDtos.Length + 1));

        var newlyAddedServer = servers.FirstOrDefault(x => x.Metadata?.Name == testNewServer.Metadata.Name);
        Assert.That(newlyAddedServer, Is.Not.Null);

        Assert.That(newlyAddedServer.Metadata?.DisplayName, Is.EqualTo(testNewServer.Metadata?.DisplayName));
        Assert.That(newlyAddedServer.Metadata?.Name, Is.EqualTo(testNewServer.Metadata?.Name));
        Assert.That(newlyAddedServer.Metadata?.IPAddress, Is.EqualTo(testNewServer.Metadata?.IPAddress));
        Assert.That(newlyAddedServer.Metadata?.Description, Is.EqualTo(testNewServer.Metadata?.Description));
        Assert.That(newlyAddedServer.Metadata?.Kind, Is.EqualTo(testNewServer.Metadata?.Kind));

        //TODO: Fill in when docker compose support is added.
        //TODO: FIll in when server configuration support is added.
    }

    /// <summary>
    /// Testing server metadata.
    /// </summary>
    private static readonly ServerMetadataDto[] _metadataDtos = new[]
    {
        new ServerMetadataDto()
        {
            DisplayName = "server 1",
            Name = "server1",
            IPAddress = "192.168.1.1",
            Description = "Server 1 Description",
            Kind = ServerKind.WindowsWSL,
        },
        new ServerMetadataDto()
        {
            DisplayName = "server 2",
            Name = "server2",
            IPAddress = "192.168.1.2",
            Description = "Server 2 Description",   
            Kind = ServerKind.Ubuntu,
        },
        new ServerMetadataDto()
        {
            DisplayName = "server 3",
            Name = "server3",
            IPAddress = "192.168.1.3",
            Description = "Server 3 Description",
            Kind = ServerKind.WindowsWSL,
        },
    };
}
