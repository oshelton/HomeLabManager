﻿using System.Runtime.CompilerServices;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Services.Logging;
using LibGit2Sharp;

[assembly: InternalsVisibleTo("HomeLabManager.DataTests")]

namespace HomeLabManager.Common.Data.Git.Server;

/// <inheritdoc />
public sealed class ServerDataManager : IServerDataManager
{
    /// <inheritdoc />
    public ServerDataManager(ICoreConfigurationManager coreConfigurationManager, ILogManager logManager)
    {
        _coreConfigurationManager = coreConfigurationManager ?? throw new ArgumentNullException(nameof(coreConfigurationManager));

        _logManager = logManager?.CreateContextualizedLogManager<ServerDataManager>() ?? throw new ArgumentNullException(nameof(logManager));
        _logManager.GetApplicationLogger().Information("Created");
    }

    /// <inheritdoc />
    public IReadOnlyList<ServerHostDto> GetServers()
    {
        var logger = _logManager.GetApplicationLogger();

        var coreConfiguration = _coreConfigurationManager.GetActiveCoreConfiguration();
        if (!Directory.Exists(coreConfiguration.HomeLabRepoDataPath))
        {
            logger.Warning("Home lab repo path \"{RepoPath}\" does not exist", coreConfiguration.HomeLabRepoDataPath);
            return Array.Empty<ServerHostDto>();
        }

        T readServerDto<T>(string basePath) where T : BaseServerDto, new()
        {
            logger.Information("Reading server information from files in \"{BasePath}\"", basePath);

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

            // TODO: Add VM support.

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

        using var _ = _logManager.StartTimedOperation("Reading Servers From Repo Directory");

        var foundServerDtos = new List<ServerHostDto>();
        var repoPath = _coreConfigurationManager.GetActiveCoreConfiguration().HomeLabRepoDataPath;
        var serversDir = Path.Combine(repoPath, ServersDirectoryName);
        if (Directory.Exists(serversDir))
        {
            foreach (var serverDirectory in Directory.EnumerateDirectories(serversDir))
            {
                var serverHostDto = readServerDto<ServerHostDto>(serverDirectory);
                serverHostDto.VMs = Directory.EnumerateDirectories(serverDirectory, $"{ServerVmDto.UniqueIdPrefix}*").Select(readServerDto<ServerVmDto>).ToList();
                foundServerDtos.Add(serverHostDto);
            }
        }

        return foundServerDtos;
    }

    /// <inheritdoc />
    public void AddUpdateServer(ServerHostDto server)
    {
        if (!Directory.Exists(_coreConfigurationManager.GetActiveCoreConfiguration().HomeLabRepoDataPath))
            throw new InvalidOperationException("Server cannot be added if the repo data path directory does not exist.");

        if (server is null)
            throw new ArgumentNullException(nameof(server));

        if (server.Metadata is null)
            throw new InvalidDataException("Server must at least have metadata assigned to it.");

        var logger = _logManager.GetApplicationLogger();

        var repoPath = _coreConfigurationManager.GetActiveCoreConfiguration().HomeLabRepoDataPath;
        var serversDirectory = Path.Combine(repoPath, ServersDirectoryName);

        logger.Information("Updating server in directory \"{ServerDirectory}\" with unique Id \"{UniqueId}\"", serversDirectory, server.UniqueId);

        if (!Directory.Exists(serversDirectory))
        {
            logger.Information("Servers Directory does not exist, creating");
            Directory.CreateDirectory(serversDirectory);
        }

        using var _ = _logManager.StartTimedOperation("Adding or Updating Server");
        WriteServerHostDto(server, serversDirectory, _logManager);
    }

    /// <inheritdoc />
    public void DeleteServer(ServerHostDto server)
    {
        if (!Directory.Exists(_coreConfigurationManager.GetActiveCoreConfiguration().HomeLabRepoDataPath!))
            throw new InvalidOperationException("Server cannot be deleted if the repo data path directory does not exist.");

        if (server is null)
            throw new ArgumentNullException(nameof(server));

        if (server.UniqueId is null)
            throw new InvalidDataException("This server doess not have an Id and should be added, not deleted.");

        var repoPath = _coreConfigurationManager.GetActiveCoreConfiguration().HomeLabRepoDataPath!;
        var serverDirectory = Path.Combine(repoPath, ServersDirectoryName, server.UniqueIdToDirectoryName());

        _logManager.GetApplicationLogger().Information("Deleting server with Unique Id \"{UniqueId}\" and directory \"{serverDirectory}\"", server.UniqueId, serverDirectory);

        if (!Directory.Exists(serverDirectory))
            throw new InvalidOperationException("The directory for this server does not exist and cannot be deleted.");

        using var _ = _logManager.StartTimedOperation("Deleting Server");
        Directory.Delete(serverDirectory, true);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> MapChangesToHumanReadableInfo(RepositoryStatus status)
    {
        if (status is null)
            throw new ArgumentNullException(nameof(status));

        var results = new List<string>();
        var servers = GetServers();

        var allChanges = status.Untracked
            .Concat(status.Added)
            .Concat(status.Modified)
            .Concat(status.Removed)
            .Concat(status.Missing)
            .Concat(status.Staged)
            .Concat(status.Untracked)
            .DistinctBy(x => x.FilePath)
            .GroupBy(x => x.State);

        foreach (var statusChanges in allChanges)
        {
            var remapped = MapEntries(statusChanges);
            if (remapped is null)
            {
                _logManager.GetApplicationLogger().Warning("Unhandled change type encountered {ChangeType} for one or more files.", statusChanges.Key);
            }
            else
            {
                var serverChanges = remapped?.Entries.Where(x => x.FilePath.StartsWith(ServersDirectoryName, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(x => Path.GetDirectoryName(x.FilePath.Substring(ServersDirectoryName.Length + 1)))
                    .ToArray();

                foreach (var serverGroup in serverChanges)
                {
                    var matchingServer = servers.FirstOrDefault(x => x.UniqueId?.ToString() == serverGroup.Key);
                    if (matchingServer is null)
                    {
                        results.Add($"Deleted Server host with Unique Id: {serverGroup.Key}");
                    }
                    else
                    {
                        switch (remapped?.Change)
                        {
                            case ChangeKind.Added:
                                var existingFiles = Directory.GetFiles(matchingServer.Directory, "*.*", SearchOption.AllDirectories);
                                if (existingFiles.Length == serverGroup.Count())
                                    results.Add($"Created new server host: {matchingServer.Metadata.DisplayName}");
                                else
                                    results.Add($"Updating server host with new features: {matchingServer.Metadata.DisplayName}");
                                break;
                            case ChangeKind.Modified:
                                results.Add($"Modified server host: {matchingServer.Metadata.DisplayName}");
                                break;
                            case ChangeKind.Removed:
                                results.Add($"Removing features from server host: {matchingServer.Metadata.DisplayName}");
                                break;
                        }
                    }
                }
            }
        }

        return results;
    }

    private static (ChangeKind Change, IReadOnlyList<StatusEntry> Entries)? MapEntries(IGrouping<FileStatus, StatusEntry> entries)
    {
        return entries switch
        {
            IGrouping<FileStatus, StatusEntry> grouping when grouping.Key == FileStatus.NewInWorkdir || entries.Key == FileStatus.NewInIndex => (ChangeKind.Added, grouping.ToArray()),
            IGrouping<FileStatus, StatusEntry> grouping when grouping.Key == FileStatus.ModifiedInWorkdir || entries.Key == FileStatus.ModifiedInIndex => (ChangeKind.Modified, grouping.ToArray()),
            IGrouping<FileStatus, StatusEntry> grouping when grouping.Key == FileStatus.DeletedFromWorkdir || entries.Key == FileStatus.DeletedFromIndex => (ChangeKind.Removed, grouping.ToArray()),
            _ => null,
        };
    }

    private enum ChangeKind
    {
        Added,
        Modified,
        Removed
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
    private static void WriteServerHostDto(ServerHostDto server, string serversDirectory, ContextAwareLogManager<ServerDataManager> logManager)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));
        if (serversDirectory is null)
            throw new ArgumentNullException(nameof(serversDirectory));

        var logger = logManager.GetApplicationLogger();

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

        logger.Information("Writing Server Host \"{UniqueId}\" to directory \"{VMDirectory}\"", server.UniqueId, newServerDirectory);

        writeDtoToFile(server, newServerDirectory);

        foreach (var vm in server.VMs)
        {
            vm.UniqueId ??= Guid.NewGuid();

            var newVmDirectory = Path.Combine(newServerDirectory, vm.UniqueIdToDirectoryName()!);
            if (!Directory.Exists(newVmDirectory))
                Directory.CreateDirectory(newVmDirectory);

            logger.Information("Writing VM DTO \"{UniqueId}\" to directory \"{VMDirectory}\"", vm.UniqueId, newVmDirectory);

            writeDtoToFile(vm, newVmDirectory);
        }
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly ContextAwareLogManager<ServerDataManager> _logManager;
}
