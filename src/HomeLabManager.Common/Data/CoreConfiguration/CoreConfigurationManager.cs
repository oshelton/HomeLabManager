using System.Reactive.Subjects;
using HomeLabManager.Common.Services.Logging;
using Serilog;

namespace HomeLabManager.Common.Data.CoreConfiguration;

/// <summary>
/// Class for managing the core configuration file related to this project.
/// </summary>
/// <remarks>References to CoreConfigurationDtos should not be held onto but rather requested when needed.</remarks>
public class CoreConfigurationManager : ICoreConfigurationManager
{
    /// <summary>
    /// Construct a new CoreConfigurationManager given the directory where the core configurationfile should live.
    /// </summary>
    /// <remarks>The class will append the expected file name to the passed in directory path.</remarks>
    public CoreConfigurationManager(string coreConfigDirectory, ILogManager logManager)
    {
        if (string.IsNullOrEmpty(coreConfigDirectory))
            throw new ArgumentNullException(nameof(coreConfigDirectory));
        if (!Directory.Exists(coreConfigDirectory))
            throw new InvalidOperationException($"{nameof(coreConfigDirectory)} must exist.");

        CoreConfigPath = Path.Combine(coreConfigDirectory, "CoreConfig.yaml");
        CoreConfigurationUpdated = new Subject<CoreConfigurationDto>();

        _logManager = logManager?.CreateContextualizedLogManager<CoreConfigurationManager>() ?? throw new ArgumentNullException(nameof(logManager));
        _logManager.GetApplicationLogger().Information("Created with config directory \"{ConfigDir}\"", coreConfigDirectory);
    }

    /// <summary>
    /// Get or create the core configuration file if it doesn't exist.
    /// </summary>
    /// <param name="defaultGenerator">Generator function used to create the initial configuration.</param>
    /// <remarks>This method should only be called when the application is first started.</remarks>
    public CoreConfigurationDto GetOrCreateCoreConfiguration(Func<CoreConfigurationDto> defaultGenerator)
    {
        if (defaultGenerator is null)
            throw new ArgumentNullException(nameof(defaultGenerator));

        var logger = _logManager.GetApplicationLogger();

        if (!File.Exists(CoreConfigPath))
        {
            var defaultConfiguration = defaultGenerator();
            logger.Information("Core configuration does not exist, creating a new one: {Initial}.", defaultConfiguration);
            if (!DisableConfigurationCaching)
                _cachedCoreConfiguration = defaultConfiguration;

            var serializer = DataUtils.CreateBasicYamlSerializer();

            File.WriteAllText(CoreConfigPath, serializer.Serialize(defaultConfiguration));
            return defaultConfiguration;
        }
        else
        {
            logger.Information("Getting core configuration from file.");
            var deserializer = DataUtils.CreateBasicYamlDeserializer();

            var readConfiguration = deserializer.Deserialize<CoreConfigurationDto>(File.ReadAllText(CoreConfigPath))!;
            _cachedCoreConfiguration = readConfiguration;
            return readConfiguration;
        }
    }

    /// <summary>
    /// Get an existing core configuration object.
    /// </summary>
    /// <remarks>This should be called in most places that need access to the core configuration.</remarks>
    public CoreConfigurationDto GetCoreConfiguration()
    {
        var logger = _logManager.GetApplicationLogger();
        if (_cachedCoreConfiguration is not null && !DisableConfigurationCaching)
        {
            logger.Verbose("Getting cached core configuration.");
            return _cachedCoreConfiguration;
        }

        logger.Information("Getting core configuration from file.");
        var deserializer = DataUtils.CreateBasicYamlDeserializer();

        var readConfiguration = deserializer.Deserialize<CoreConfigurationDto>(File.ReadAllText(CoreConfigPath));
        if (!DisableConfigurationCaching)
            _cachedCoreConfiguration = readConfiguration;

        return readConfiguration;
    }

    /// <summary>
    /// Save an updated core configuration object to disk.
    /// </summary>
    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration)
    {
        if (updatedConfiguration is null)
            throw new ArgumentNullException(nameof(updatedConfiguration));

        _logManager.GetApplicationLogger().Information("Saving updated core configuration {Configuration}", updatedConfiguration);

        if (!DisableConfigurationCaching)
            _cachedCoreConfiguration = updatedConfiguration;

        var serializer = DataUtils.CreateBasicYamlSerializer();
        File.WriteAllText(CoreConfigPath, serializer.Serialize(updatedConfiguration));

        CoreConfigurationUpdated.OnNext(updatedConfiguration);
    }

    /// <summary>
    /// Path tot he core configuratiuon file.
    /// </summary>
    public string CoreConfigPath { get; }

    /// <summary>
    /// Disable core config in memory caching; should only be used for testing purposes.
    /// </summary>
    /// <remarks>This assumes that the application</remarks>
    public bool DisableConfigurationCaching { get; set; }

    /// <summary>
    /// Subject that publishes the new configuration to consumers when it is updated.
    /// </summary>
    /// <remarks>Dtos should be considered transient and not held onto; including this one.</remarks>
    public Subject<CoreConfigurationDto> CoreConfigurationUpdated { get; }

    private readonly ContextAwareLogManager<CoreConfigurationManager> _logManager;

    private CoreConfigurationDto _cachedCoreConfiguration;
}
