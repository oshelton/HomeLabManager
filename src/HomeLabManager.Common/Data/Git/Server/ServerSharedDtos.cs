using YamlDotNet.Serialization;

namespace HomeLabManager.Common.Data.Git.Server;

/// <summary>
/// Kind of server.
/// </summary>
public enum ServerKind
{
    Windows,
    StandardLinux,
    TrueNas,
}

/// <summary>
/// Server Metadata information DTO.
/// </summary>
public sealed record ServerMetadataDto
{
    /// <summary>
    /// Path to the file the metadata came from.
    /// </summary>
    [YamlIgnore]
    public string FileName { get; init; }

    /// <summary>
    /// Display name to use with the server.
    /// </summary>
    public string DisplayName { get; init; }
    /// <summary>
    /// Computer/Host name of the server.
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// IP Address of the server; should be DHCP reserved or static for best results.
    /// </summary>
    public string IPAddress { get; set; }
    /// <summary>
    /// Description of the server.
    /// </summary>
    public string Description { get; init; }
    /// <summary>
    /// Underlying kind of the server, controls how the server can be nteracted with.
    /// </summary>
    public ServerKind? Kind { get; init; }
}

/// <summary>
/// DTO corresponding to the docker compose file of this server.
/// </summary>
public sealed record DockerComposeDto
{

}

/// <summary>
/// DTO for the server's configuration.
/// </summary>
public sealed record ServerConfigurationDto
{

}

public abstract record BaseServerDto
{
    /// <summary>
    /// Name of the directory the server dto came from.
    /// </summary>
    public string Directory { get; init; }

    /// <summary>
    /// Unique id of this server, is the name of the directory the server lives in.
    /// </summary>
    public Guid? UniqueId { get; internal set; }

    /// <summary>
    /// Metadata for this server.
    /// </summary>
    public ServerMetadataDto Metadata { get; init; }
    /// <summary>
    /// Data extracted from the server's Docker compose file.
    /// </summary>
    public DockerComposeDto DockerCompose { get; init; }
    /// <summary>
    /// Server configuration details of the server.
    /// </summary>
    public ServerConfigurationDto Configuration { get; init; }

    /// <summary>
    /// Convert the Server's unique ID to a string capable of being used as a directory name.
    /// </summary>
    /// <returns>A directory safe string or null if the server has no unique ID.</returns>
    public virtual string UniqueIdToDirectoryName() => UniqueId?.ToString("D");
}
