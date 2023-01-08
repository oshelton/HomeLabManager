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
    public CoreConfigurationDto GetOrCreateCoreConfiguration(Func<CoreConfigurationDto> defaultGenerator);

    /// <summary>
    /// Get an existing core configuration object.
    /// </summary>
    /// <remarks>This should be called in most places that need access to the core configuration.</remarks>
    public CoreConfigurationDto GetCoreConfiguration();

    /// <summary>
    /// Save an updated core configuration object to disk.
    /// </summary>
    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration);

    /// <summary>
    /// Path tot he core configuratiuon file.
    /// </summary>
    public string CoreConfigPath { get; }

    /// <summary>
    /// Disable core config in memory caching; should only be used for testing purposes.
    /// </summary>
    /// <remarks>This assumes that the application</remarks>
    public bool DisableConfigurationCaching { get; set; }
}
