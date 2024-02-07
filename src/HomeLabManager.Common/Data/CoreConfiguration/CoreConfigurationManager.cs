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

        _coreConfigDir = coreConfigDirectory;

        ActiveCoreConfigPath = GetActiveConfigurationPath(GetAllCoreConfigurationPaths());
        CoreConfigurationUpdated = new Subject<CoreConfigurationDto>();

        _logManager = logManager?.CreateContextualizedLogManager<CoreConfigurationManager>() ?? throw new ArgumentNullException(nameof(logManager));
        _logManager.GetApplicationLogger().Information("Created with config directory \"{ConfigDir}\"", coreConfigDirectory);
    }

    /// <inheritdoc />
    public CoreConfigurationDto GetOrCreateActiveCoreConfiguration(Func<CoreConfigurationDto> defaultGenerator)
    {
        if (defaultGenerator is null)
            throw new ArgumentNullException(nameof(defaultGenerator));

        var logger = _logManager.GetApplicationLogger();

        if (ActiveCoreConfigPath is null)
        {
            var defaultConfiguration = defaultGenerator();
            if (defaultConfiguration.Name is null)
                throw new InvalidOperationException("Default configuration must have a name.");

            // Default configuration must always start out active.
            defaultConfiguration.IsActive = true;
            defaultConfiguration.InitialName = defaultConfiguration.Name;
            defaultConfiguration.InitialIsActive = defaultConfiguration.IsActive;
            defaultConfiguration.FilePath = Path.Combine(_coreConfigDir, BuildFileNameForConfiguration(defaultConfiguration));
            ActiveCoreConfigPath = defaultConfiguration.FilePath;

            logger.Information("Core configuration does not exist, creating a new one: {Initial}.", defaultConfiguration);

            using (_logManager.StartTimedOperation("Core Configuration Default Serialization"))
            {
                var serializer = DataUtils.CreateBasicYamlSerializer();

                File.WriteAllText(ActiveCoreConfigPath, serializer.Serialize(defaultConfiguration));
            }
            return defaultConfiguration;
        }
        else
        {
            return DeserializeConfigurationFromFile(ActiveCoreConfigPath);
        }
    }

    /// <inheritdoc />
    public CoreConfigurationDto GetActiveCoreConfiguration() => DeserializeConfigurationFromFile(ActiveCoreConfigPath);

    /// <inheritdoc />
    public CoreConfigurationDto GetCoreConfiguration(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Must not be null or empty.", nameof(name));

        var matchingPath = GetAllCoreConfigurationPaths().FirstOrDefault(x => Path.GetFileName(x).Contains($"{name}.", StringComparison.InvariantCultureIgnoreCase)) 
            ?? throw new InvalidOperationException($"No configuration was found with name \"{name}\"");

        return DeserializeConfigurationFromFile(matchingPath);
    }

    /// <inheritdoc />
    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration)
    {
        if (updatedConfiguration is null)
            throw new ArgumentNullException(nameof(updatedConfiguration));
        if (string.IsNullOrEmpty(updatedConfiguration.Name))
            throw new ArgumentException("Must not be null or empty.", nameof(updatedConfiguration));

        _logManager.GetApplicationLogger().Information("Saving updated core configuration {Configuration}", updatedConfiguration);

        if (updatedConfiguration.IsActive && !updatedConfiguration.InitialIsActive && ActiveCoreConfigPath is not null)
        {
            var previousActiveConfig = GetActiveCoreConfiguration();
            previousActiveConfig.IsActive = false;
            SaveCoreConfiguration(previousActiveConfig);
        }

        using (_logManager.StartTimedOperation("Core Configuration Serialization and Writing"))
        {
            var wasActive = updatedConfiguration.InitialIsActive;

            WriteConfigurationToFile(updatedConfiguration);

            if (updatedConfiguration.IsActive)
                ActiveCoreConfigPath = updatedConfiguration.FilePath;
            else if (wasActive && !updatedConfiguration.IsActive)
                ActiveCoreConfigPath = null;
        }

        CoreConfigurationUpdated.OnNext(updatedConfiguration);
    }

    /// <inheritdoc />
    public IReadOnlyList<(string Name, bool IsActive)> GetAllCoreConfigurations()
        => GetAllCoreConfigurationPaths().Select(x =>
        {
            var fileName = Path.GetFileName(x);
            var firstPeriodIndex = fileName.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);
            return (fileName[..firstPeriodIndex], IsPathActiveConfigurationFile(x));
        }).OrderBy(x => x.Item1).ToArray();

    /// <inheritdoc/>
    public void DeleteCoreConfiguration(CoreConfigurationDto configuration)
    {
        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));
        if (configuration.IsActive)
            throw new InvalidOperationException("Cannot delete an active configuration.");
        if (!File.Exists(configuration.FilePath))
            throw new InvalidOperationException($"File \"{configuration.FilePath}\" does not exist and there is nothing to be deleted.");

        File.Delete(configuration.FilePath);
    }

    /// <inheritdoc />
    public string ActiveCoreConfigPath { get; private set; }

    /// <inheritdoc />
    public Subject<CoreConfigurationDto> CoreConfigurationUpdated { get; }

    /// <summary>
    /// Get the path to the currently active configuration file.
    /// </summary>
    private static string GetActiveConfigurationPath(IReadOnlyList<string> configurationPaths) => configurationPaths.SingleOrDefault(x => IsPathActiveConfigurationFile(x));

    /// <summary>
    /// Determine if the passed in path represents an active configuration file.
    /// </summary>
    private static bool IsPathActiveConfigurationFile(string path) => Path.GetFileName(path).Contains($".{CoreConfigFileActivePart}.", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Build the file name for the provided configuration.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static string BuildFileNameForConfiguration(CoreConfigurationDto configuration) => $"{configuration.Name}.{CoreConfigFilePrefix}{(configuration.IsActive ? $".{CoreConfigFileActivePart}" : "")}.{CoreConfigFileExtension}";

    /// <summary>
    /// Deserialize a core configuration object from file.
    /// </summary>
    private CoreConfigurationDto DeserializeConfigurationFromFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("Must not be null or empty.", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} must exist.");

        var deserializer = DataUtils.CreateBasicYamlDeserializer();

        using (_logManager.StartTimedOperation("Core Configuration Reading and Deserialization"))
        {
            var readConfiguration = deserializer.Deserialize<CoreConfigurationDto>(File.ReadAllText(filePath))
                ?? throw new InvalidOperationException($"Error deserializing core configuration file at \"{filePath}\"");

            var fileName = Path.GetFileName(filePath);
            var firstPeriodIndex = fileName.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);

            readConfiguration.Name = fileName[..firstPeriodIndex];
            readConfiguration.IsActive = IsPathActiveConfigurationFile(filePath);
            readConfiguration.InitialName = readConfiguration.Name;
            readConfiguration.InitialIsActive = readConfiguration.IsActive;
            readConfiguration.FilePath = filePath;

            return readConfiguration;
        }
    }

    /// <summary>
    /// Write the configuration to a file.
    /// </summary>
    private void WriteConfigurationToFile(CoreConfigurationDto configuration)
    {
        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        if (configuration.FilePath is not null && ((configuration.Name != configuration.InitialName && configuration.InitialName is not null) || configuration.IsActive != configuration.InitialIsActive))
            File.Delete(configuration.FilePath);

        var outputPath = Path.Combine(_coreConfigDir, BuildFileNameForConfiguration(configuration));

        if (GetAllCoreConfigurations().Any(x => x.Name == configuration.Name) && configuration.Name != configuration.InitialName)
            throw new InvalidOperationException($"Another configuration with the same name \"{configuration.Name}\" already exists, nothing will be saved!");

        var serializer = DataUtils.CreateBasicYamlSerializer();
        File.WriteAllText(outputPath, serializer.Serialize(configuration));

        configuration.InitialName = configuration.Name;
        configuration.InitialIsActive = configuration.IsActive;
        configuration.FilePath = outputPath;
    }

    /// <summary>
    /// Get full paths to all available core configuration files.
    /// </summary>
    private IReadOnlyList<string> GetAllCoreConfigurationPaths() => Directory.GetFiles(_coreConfigDir, $"*.{CoreConfigFilePrefix}.*").ToArray();

    private const string CoreConfigFilePrefix = "CoreConfig";
    private const string CoreConfigFileActivePart = "Active";
    private const string CoreConfigFileExtension = "yaml";

    private readonly string _coreConfigDir;
    private readonly ContextAwareLogManager<CoreConfigurationManager> _logManager;
}
