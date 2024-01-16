using YamlDotNet.Serialization;

namespace HomeLabManager.Common.Data.CoreConfiguration;

/// <summary>
/// Simple DTO for the core application configuration.
/// </summary>
/// <remarks>Dtos should be considered transient and not held onto.</remarks>
public sealed record CoreConfigurationDto
{
    /// <summary>
    /// Name of the configuration.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Path to the Home Lab Git Repo.
    /// </summary>
    public string HomeLabRepoDataPath { get; init; }

    /// <summary>
    /// Path to the git config file from which to extract the user name and email used when committing data.
    /// </summary>
    public string GitConfigFilePath { get; init; }

    /// <summary>
    /// Github user name.
    /// </summary>
    public string GithubUserName { get; init; }

    /// <summary>
    /// Github Personal Access Token.
    /// </summary>
    /// <remarks>See https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token for details.</remarks>
    public string GithubPat { get; init; }

    /// <summary>
    /// Whether or not this configuration is active.
    /// </summary>
    [YamlIgnore]
    public bool IsActive { get; internal set; }

    /// <summary>
    /// Name portion of the file the configuration came from.
    /// </summary>
    [YamlIgnore]
    internal string InitialName { get; set; }

    /// <summary>
    /// Whether or not the configuration was initially active.
    /// </summary>
    [YamlIgnore]
    internal bool InitialIsActive { get; set; }

    /// <summary>
    /// Underlying path to the file as when loaded.
    /// </summary>
    [YamlIgnore]
    internal string FilePath { get; set; }
}
