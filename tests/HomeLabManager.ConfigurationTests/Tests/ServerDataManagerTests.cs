using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Services.Logging;
using LibGit2Sharp;

namespace HomeLabManager.DataTests.Tests;

public sealed class ServerDataManagerTests
{
    [SetUp]
    public void SetUp()
    {
        Directory.CreateDirectory(Utils.TestDirectory);

        var info = Directory.CreateDirectory(Utils.TestGitDirectory);
        info.Attributes &= ~FileAttributes.ReadOnly;
        File.SetAttributes(Utils.TestDirectory, info.Attributes);

        var serializer = Common.Data.DataUtils.CreateBasicYamlSerializer();
        foreach (var server in _servers)
        {
            var directoryName = server.UniqueId?.ToString("D")!;
            var directory = Path.Combine(Utils.TestGitDirectory, ServerDataManager.ServersDirectoryName, directoryName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, ServerDataManager.ServerMetadataFileName), serializer.Serialize(server.Metadata!));

            foreach (var vm in server.VMs) 
            {
                var vmDirectoryName = ServerVmDto.UniqueIdPrefix + vm.UniqueId?.ToString("D")!;
                var vmDirectory = Path.Combine(directory, vmDirectoryName);
                Directory.CreateDirectory(vmDirectory);
                File.WriteAllText(Path.Combine(vmDirectory, ServerDataManager.ServerMetadataFileName), serializer.Serialize(vm.Metadata!));
            }
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
    public void GetServers_CompareReadWithTruth_ConfirmServersReadFromFilesMatchExpected()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager, new LogManager(true));

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

        var serverWithVms = servers.First(x => x.VMs.Count > 0);
        var testVmServer = _servers.First(x => x.VMs.Count > 0);

        Assert.That(serverWithVms.VMs, Has.Count.EqualTo(testVmServer.VMs.Count));
        foreach (var vm in serverWithVms.VMs)
        {
            var testVm = testVmServer.VMs.First(x => x.UniqueId == vm.UniqueId);

            Assert.That(vm.Directory, Is.EqualTo(Path.Combine(serverWithVms.Directory!, ServerVmDto.UniqueIdPrefix + testVm.UniqueId!.ToString()!)));
            Assert.That(vm.Metadata?.FileName, Is.EqualTo(Path.Combine(vm.Directory, ServerDataManager.ServerMetadataFileName)));
            Assert.That(vm.Metadata?.DisplayName, Is.EqualTo(testVm.Metadata?.DisplayName));
            Assert.That(vm.Metadata?.Name, Is.EqualTo(testVm.Metadata?.Name));
            Assert.That(vm.Metadata?.IPAddress, Is.EqualTo(testVm.Metadata?.IPAddress));
            Assert.That(vm.Metadata?.Description, Is.EqualTo(testVm.Metadata?.Description));
            Assert.That(vm.Metadata?.Kind, Is.EqualTo(testVm.Metadata?.Kind));
            Assert.That(vm.Host, Is.SameAs(serverWithVms));
        }
    }

    /// <summary>
    /// Test adding a new server.
    /// </summary>
    [Test]
    public void AddUpdateServer_AddNewServer_ConfirmAddingNewServerWorks()
    {
        var testNewServer = new ServerHostDto()
        {
            Metadata = new ServerMetadataDto()
            {
                DisplayName = "server 4",
                Name = "server4",
                IPAddress = "192.168.1.5",
                Description = "Server 4 Description",
                Kind = ServerKind.StandardLinux,
            },
            VMs = new []
            {
                new ServerVmDto() 
                {
                    Metadata = new ServerMetadataDto
                    {
                        DisplayName = "server 4 vm 1",
                        Name = "vm 1",
                        Kind = ServerKind.StandardLinux,
                    },
                    DockerCompose = null, //TODO: Fill in when docker compose support is added.
                    Configuration = null, //TODO: FIll in when server configuration support is added.
                }
            },
            DockerCompose = null, //TODO: Fill in when docker compose support is added.
            Configuration = null, //TODO: FIll in when server configuration support is added.
        };

        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager, new LogManager(true));
        serverManager.AddUpdateServer(testNewServer);
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

        Assert.That(newlyAddedServer.VMs, Has.Count.EqualTo(testNewServer?.VMs.Count));
        Assert.That(newlyAddedServer.VMs[0], Is.Not.Null);
        Assert.That(newlyAddedServer.VMs[0].Metadata!.DisplayName, Is.EqualTo(testNewServer!.VMs[0].Metadata!.DisplayName));
        Assert.That(newlyAddedServer.VMs[0].Metadata!.Name, Is.EqualTo(testNewServer!.VMs[0].Metadata!.Name));
        Assert.That(newlyAddedServer.VMs[0].Metadata!.Kind, Is.EqualTo(testNewServer!.VMs[0].Metadata!.Kind));

        //TODO: Fill in when docker compose support is added.
        //TODO: FIll in when server configuration support is added.
    }

    [Test]
    public void AddUpdateServer_UpdateExistingServer_ConfirmUpdatingExistingServerWorks()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager, new LogManager(true));

        var servers = serverManager.GetServers();

        var toUpdate = servers[0];
        toUpdate = toUpdate with
        {
            Metadata = toUpdate.Metadata! with { Name = "Updated Server Name" }
        };

        serverManager.AddUpdateServer(toUpdate);

        servers = serverManager.GetServers();

        var matchingServer = servers.First(x => x.UniqueId == toUpdate.UniqueId);

        Assert.That(matchingServer.Metadata!.Name, Is.EqualTo(toUpdate.Metadata.Name));

        //TODO: Fill in when docker compose support is added.
        //TODO: FIll in when server configuration support is added.
    }

    [Test]
    public void DeleteServer_DeleteExistingServer_ConfirmDeletingServerWorks()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager, new LogManager(true));

        var servers = serverManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_servers.Length));

        var toDelete = servers[0];

        serverManager.DeleteServer(toDelete);

        servers = serverManager.GetServers();

        Assert.That(servers.Count, Is.EqualTo(_servers.Length - 1));
        Assert.That(servers.All(x => x.UniqueId != toDelete.UniqueId), Is.True);
    }

    [Test]
    public void DeleteServer_DeleteUnsavedServer_ConfirmDeletingUnsavedServerThrows()
    {
        var serverManager = new ServerDataManager(Utils.CreateCoreConfigurationManager(false).manager, new LogManager(true));

        var newServer = new ServerHostDto();

        Assert.Throws<InvalidDataException>(() => serverManager.DeleteServer(newServer));
    }

    /// <summary>
    /// Testing server dtos.
    /// </summary>
    private static readonly ServerHostDto[] _servers = new[]
    {
        new ServerHostDto()
        {
            UniqueId = Guid.NewGuid(),
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
            UniqueId = Guid.NewGuid(),
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
            UniqueId = Guid.NewGuid(),
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
                    UniqueId = Guid.NewGuid(),
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
                    UniqueId = Guid.NewGuid(),
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
