﻿using System.Runtime.CompilerServices;
using HomeLabManager.Common.Data.CoreConfiguration;

[assembly: InternalsVisibleTo("HomeLabManager.DataTests")]

namespace HomeLabManager.Common.Data.Git.Server;

/// <inheritdoc/>
public sealed class ServerDataManager : IServerDataManager
{
    /// <inheritdoc/>
    public ServerDataManager(ICoreConfigurationManager coreConfigurationManager)
    {
        if (coreConfigurationManager is null)
            throw new ArgumentNullException(nameof(coreConfigurationManager));

        _coreConfigurationManager = coreConfigurationManager;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ServerHostDto> GetServers()
    {
        if (!Directory.Exists(_coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!))
            return Array.Empty<ServerHostDto>();

        static T readServerDto<T>(string basePath) where T : BaseServerDto, new()
        {
            var metadataPath = Path.Combine(basePath, ServerMetadataFileName);
            var dockerPath = Path.Combine(basePath, ServerDockerFileName);
            var configurationPath = Path.Combine(basePath, ServerConfigurationFileName);

            var deserializer = DataUtils.CreateBasicYamlDeserializer();

            var metadata = File.Exists(metadataPath)
                ? deserializer.Deserialize<ServerMetadataDto>(File.ReadAllText(metadataPath)) with { FileName = metadataPath }
                : new ServerMetadataDto() { Name = Path.GetDirectoryName(basePath), FileName = metadataPath };

            var docker = File.Exists(dockerPath)
                ? deserializer.Deserialize<DockerComposeDto>(File.ReadAllText(dockerPath))
                : null;

            var configuration = File.Exists(configurationPath)
                ? deserializer.Deserialize<ServerConfigurationDto>(File.ReadAllText(configurationPath))
                : null;

            var directoryName = Path.GetFileName(basePath);
            return new T
            {
                Directory = basePath,
                UniqueId = directoryName.StartsWith(ServerVmDto.UniqueIdPrefix, StringComparison.InvariantCulture) ? Guid.Parse(directoryName.AsSpan(ServerVmDto.UniqueIdPrefix.Length)) : Guid.Parse(directoryName),
                Metadata = metadata,
                DockerCompose = docker,
                Configuration = configuration
            };
        };

        var foundServerDtos = new List<ServerHostDto>();
        var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath;
        var serversDir = Path.Combine(repoPath, ServersDirectoryName);
        if (Directory.Exists(serversDir))
        {
            foreach (var serverDirectory in Directory.EnumerateDirectories(serversDir))
            {
                var serverHostDto = readServerDto<ServerHostDto>(serverDirectory);
                serverHostDto.VMs = Directory.EnumerateDirectories(serverDirectory, $"{ServerVmDto.UniqueIdPrefix}*").Select(vmDir =>
                {
                    var vm = readServerDto<ServerVmDto>(vmDir);
                    vm.Host = serverHostDto;

                    return vm;
                }).ToList();
                foundServerDtos.Add(serverHostDto);
            }
        }

        return foundServerDtos;
    }

    /// <inheritdoc/>
    public void AddUpdateServer(ServerHostDto server)
    {
        if (!Directory.Exists(_coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!))
            throw new InvalidOperationException("Server cannot be added if the repo data path directory does not exist.");

        if (server is null)
            throw new ArgumentNullException(nameof(server));

        if (server.Metadata is null)
            throw new InvalidDataException("Server must at least have metadata assigned to it.");

        var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;
        var serversDirectory = Path.Combine(repoPath, ServersDirectoryName);

        if (!Directory.Exists(serversDirectory))
            Directory.CreateDirectory(serversDirectory);

        WriteServerHostDto(server, serversDirectory);
    }

    /// <inheritdoc/>
    public void DeleteServer(ServerHostDto server)
    {
        if (!Directory.Exists(_coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!))
            throw new InvalidOperationException("Server cannot be deleted if the repo data path directory does not exist.");

        if (server is null)
            throw new ArgumentNullException(nameof(server));

        if (server.UniqueId is null)
            throw new InvalidDataException("This server doess not have an Id and should be added, not deleted.");

        var repoPath = _coreConfigurationManager.GetCoreConfiguration().HomeLabRepoDataPath!;
        var serverDirectory = Path.Combine(repoPath, ServersDirectoryName, server.UniqueIdToDirectoryName()!);

        if (!Directory.Exists(serverDirectory))
            throw new InvalidOperationException("The directory for this server does not exist and cannot be deleted.");

        Directory.Delete(serverDirectory, true);
    }

    /// <summary>
    /// Name of the directory that server information lives in.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    internal const string ServersDirectoryName = "servers";

    /// <summary>
    /// File name of the server metadata file.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    internal const string ServerMetadataFileName = "metadata.yaml";
    /// <summary>
    /// File name of the server docker compose file.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    internal const string ServerDockerFileName = "docker.yaml";
    /// <summary>
    /// File name of the server configuration file.
    /// </summary>
    /// <remarks>Mostly available for testing and utility functions; use with caution.</remarks>
    internal const string ServerConfigurationFileName = "config.yaml";

    /// <summary>
    /// Write the passed in server dto to the passed in directory.
    /// </summary>
    private static void WriteServerHostDto(ServerHostDto server, string serversDirectory)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));
        if (serversDirectory is null)
            throw new ArgumentNullException(nameof(serversDirectory));

        static void writeDtoToFile(BaseServerDto dto, string basePath)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(server));
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException(nameof(basePath));

            if (!Directory.Exists(basePath))
                throw new InvalidDataException($"{basePath} must refer to an existing directory in the filesystem.");

            var serializer = DataUtils.CreateBasicYamlSerializer();

            File.WriteAllText(Path.Combine(basePath, ServerMetadataFileName), serializer.Serialize(dto.Metadata!));

            if (dto.DockerCompose is not null)
                File.WriteAllText(Path.Combine(basePath, ServerDockerFileName), serializer.Serialize(dto.DockerCompose!));

            if (dto.Configuration is not null)
                File.WriteAllText(Path.Combine(basePath, ServerDockerFileName), serializer.Serialize(dto.Configuration!));
        };

        server.UniqueId ??= Guid.NewGuid();

        var newServerDirectory = Path.Combine(serversDirectory, server.UniqueIdToDirectoryName()!);
        if (!Directory.Exists(newServerDirectory))
            Directory.CreateDirectory(newServerDirectory);

        writeDtoToFile(server, newServerDirectory);

        foreach (var vm in server.VMs)
        {
            vm.UniqueId ??= Guid.NewGuid();

            var newVmDirectory = Path.Combine(newServerDirectory, vm.UniqueIdToDirectoryName()!);
            if (!Directory.Exists(newVmDirectory))
                Directory.CreateDirectory(newVmDirectory);

            writeDtoToFile(vm, newVmDirectory);
        }
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
}
