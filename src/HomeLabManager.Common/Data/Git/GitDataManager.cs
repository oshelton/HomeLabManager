using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.CoreConfiguration;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace HomeLabManager.Common.Data.Git;

/// <summary>
/// Class for accessing and updating the data stored in a Home Lab Git Repo.
/// </summary>
public sealed class GitDataManager
{
    /// <summary>
    /// Name of the directory that server information lives in.
    /// </summary>
    /// /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    public const string ServersDirectoryName = "servers";

    /// <summary>
    /// File name of the server metadata file.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    public const string ServerMetadataFileName = "metadata.yaml";
    /// <summary>
    /// File name of the server docker compose file.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    public const string ServerDockerFileName = "docker.yaml";
    /// <summary>
    /// File name of the server configuration file.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    public const string ServerConfigurationFileName = "config.yaml";

    /// <summary>
    /// Construct a new GitDataManager with the core configuration manager to use.
    /// </summary>
    public GitDataManager(CoreConfigurationManager coreConfigurationManager)
    {
        if (coreConfigurationManager is null)
            throw new ArgumentNullException(nameof(coreConfigurationManager));

        _coreConfigurationManager= coreConfigurationManager;
    }

    /// <summary>
    /// Get a list of the server information in the git HomeLab directory.
    /// </summary>
    public IReadOnlyList<ServerDto> GetServers()
    {
        var deserializer = Utils.CreateBasicYamlDeserializer();

        var foundServerDtos = new List<ServerDto>();
        var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;
        foreach (var serverDirectory in Directory.EnumerateDirectories(Path.Combine(repoPath, "servers")))
        {
            var metadataPath = Path.Combine(serverDirectory, ServerMetadataFileName);
            var dockerPath = Path.Combine(serverDirectory, ServerDockerFileName);
            var configurationPath = Path.Combine(serverDirectory, ServerConfigurationFileName);

            ServerMetadataDto metadata = File.Exists(metadataPath)
                ? deserializer.Deserialize<ServerMetadataDto>(File.ReadAllText(metadataPath))
                : new ServerMetadataDto() { Name = Path.GetDirectoryName(serverDirectory) };

            DockerComposeDto? docker = File.Exists(dockerPath)
                ? deserializer.Deserialize<DockerComposeDto>(File.ReadAllText(dockerPath))
                : null;

            ServerConfigurationDto? configuration = File.Exists(configurationPath)
                ? deserializer.Deserialize<ServerConfigurationDto>(File.ReadAllText(configurationPath))
                : null;

            foundServerDtos.Add(new ServerDto()
            {
                Metadata = metadata,
                DockerCompose = docker,
                Configuration = configuration
            });
        }

        return foundServerDtos;
    }

    /// <summary>
    /// Add a new server to the repo.
    /// </summary>
    public void AddNewServer(ServerDto server)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));

        if (server?.Metadata is null)
            throw new InvalidDataException("Server must at least have metadata assigned to it.");
        if (server?.Metadata.Name is null)
            throw new InvalidDataException("Server to be added must have a name.");

        var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;
        var serversDirectory = Path.Combine(repoPath, ServersDirectoryName);
        if (Directory.GetDirectories(serversDirectory).Any(x => Path.GetDirectoryName(x) == server.Metadata?.Name))
            throw new InvalidDataException("A server already exists with this name; try another.");

        var newServerDirectory = Path.Combine(serversDirectory, server.Metadata?.Name!);
        Directory.CreateDirectory(newServerDirectory);

        var serializer = Utils.CreateBasicYamlSerializer();

        File.WriteAllText(Path.Combine(newServerDirectory, ServerMetadataFileName), serializer.Serialize(server.Metadata!));

        if (server.DockerCompose is not null)
            File.WriteAllText(Path.Combine(newServerDirectory, ServerDockerFileName), serializer.Serialize(server.DockerCompose!));

        if (server.Configuration is not null)
            File.WriteAllText(Path.Combine(newServerDirectory, ServerDockerFileName), serializer.Serialize(server.Configuration!));
    }

    private readonly CoreConfigurationManager _coreConfigurationManager;
}
