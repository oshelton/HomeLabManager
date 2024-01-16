using System.Reactive.Subjects;

namespace HomeLabManager.Common.Data.CoreConfiguration;

/// <summary>
/// Interface for managing the core configuration file related to this project.
/// </summary>
/// <remarks>References to CoreConfigurationDtos should not be held onto but rather requested when needed.</remarks>
public interface ICoreConfigurationManager
{
    /// <summary>
    /// Get or create the core configuration file if it doesn't exist.
    /// </summary>
    /// <param name="defaultGenerator">Generator function used to create the initial configuration.</param>
    /// <remarks>This method should only be called when the application is first started.</remarks>
    public CoreConfigurationDto GetOrCreateActiveCoreConfiguration(Func<CoreConfigurationDto> defaultGenerator);

    /// <summary>
    /// Get an existing core configuration object.
    /// </summary>
    /// <remarks>This should be called in most places that need access to the core configuration.</remarks>
    public CoreConfigurationDto GetActiveCoreConfiguration();

    /// <summary>
    /// Get the core configuration details for a given name.
    /// </summary>
    public CoreConfigurationDto GetCoreConfiguration(string name);

    /// <summary>
    /// Save an updated core configuration object to disk.
    /// </summary>
    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration);

    /// <summary>
    /// Get information for all core configurations.
    /// </summary>
    /// <returns>A collection containing the names of all configurations and a flag for which one is the active one.</returns>
    public IReadOnlyList<(string Name, bool IsActive)> GetAllCoreConfigurations();

    /// <summary>
    /// Path to the active core configuration file.
    /// </summary>
    public string ActiveCoreConfigPath { get; }

    /// <summary>
    /// Subject that publishes the new configuration to consumers when it is updated.
    /// </summary>
    /// <remarks>Dtos should be considered transient and not held onto; including this one.</remarks>
    public Subject<CoreConfigurationDto> CoreConfigurationUpdated { get; }
}
