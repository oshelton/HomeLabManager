using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeLabManager.Common.Data.Git;

/// <summary>
/// Kind of server.
/// </summary>
public enum ServerKind
{
    WindowsWSL,
    Ubuntu,
}

/// <summary>
/// DTO representing a Server's information.
/// </summary>
public sealed record ServerDto
{
    /// <summary>
    /// Metadata for this server.
    /// </summary>
    public ServerMetadataDto? Metadata { get; init; }
    /// <summary>
    /// Data extracted from the server's Docker compose file.
    /// </summary>
    public DockerComposeDto? DockerCompose { get; init; }
    /// <summary>
    /// Server configuration details of the server.
    /// </summary>
    public ServerConfigurationDto? Configuration { get; init; }
}

/// <summary>
/// Server Metadata information DTO.
/// </summary>
public sealed record ServerMetadataDto
{
    /// <summary>
    /// Display name to use with the server.
    /// </summary>
    public string? DisplayName { get; init; }
    /// <summary>
    /// Computer/Host name of the server.
    /// </summary>
    public string? Name { get; init; }
    /// <summary>
    /// IP Address of the server; should be DHCP reserved or static for best results.
    /// </summary>
    public string? IPAddress { get; set; }
    /// <summary>
    /// Description of the server.
    /// </summary>
    public string? Description { get; init; }
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
