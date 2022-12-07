using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace HomeLabManager.Common.Data.CoreConfiguration;

/// <summary>
/// Class for managing the core configuration file related to this project.
/// </summary>
/// <remarks>References to CoreConfigurationDtos should not be held onto but rather requested when needed.</remarks>
public class CoreConfigurationManager
{
    /// <summary>
    /// Construct a new CoreConfigurationManager given the directory where the core configurationfile should live.
    /// </summary>
    /// <remarks>The class will append the expected file name to the passed in directory path.</remarks>
    public CoreConfigurationManager(string coreConfigDirectory)
    {
        if (string.IsNullOrEmpty(coreConfigDirectory))
            throw new ArgumentNullException(nameof(coreConfigDirectory));
        if (!Directory.Exists(coreConfigDirectory))
            throw new InvalidOperationException($"{nameof(coreConfigDirectory)} must exist.");

        CoreConfigPath = Path.Combine(coreConfigDirectory, "CoreConfig.yaml");
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

        if (!File.Exists(CoreConfigPath))
        {
            var defaultConfiguration = defaultGenerator();
            ValidateCoreConfig(defaultConfiguration);
            if (!DisableConfigurationCaching)
                _cachedCoreConfiguration = defaultConfiguration;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            File.WriteAllText(CoreConfigPath, serializer.Serialize(defaultConfiguration));
            return defaultConfiguration;
        }
        else
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

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
        if (_cachedCoreConfiguration is not null && !DisableConfigurationCaching)
            return _cachedCoreConfiguration;

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var readConfiguration = deserializer.Deserialize<CoreConfigurationDto>(File.ReadAllText(CoreConfigPath))!;
        _cachedCoreConfiguration = readConfiguration;
        return readConfiguration;
    }

    /// <summary>
    /// Save an updated core configuration object to disk.
    /// </summary>
    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration)
    {
        ValidateCoreConfig(updatedConfiguration);
        if (!DisableConfigurationCaching)
            _cachedCoreConfiguration = updatedConfiguration;

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        File.WriteAllText(CoreConfigPath, serializer.Serialize(updatedConfiguration));
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
    /// Validate the passed in core configuration dto.
    /// </summary>
    private static void ValidateCoreConfig(CoreConfigurationDto config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));
        if (!Directory.Exists(config.HostDataPath))
            throw new InvalidDataException($"{nameof(CoreConfigurationDto)}.{nameof(config.HostDataPath)} must be a directory that exsts.");
        if (!File.Exists(config.GitConfigFilePath))
            throw new InvalidDataException($"{nameof(CoreConfigurationDto)}.{nameof(config.GitConfigFilePath)} must be a file that exsts.");
        if (string.IsNullOrEmpty(config.GithubUserName))
            throw new InvalidDataException($"{nameof(CoreConfigurationDto)}.{nameof(config.GithubUserName)} must be non-null.");
        if (string.IsNullOrEmpty(config.GithubPat))
            throw new InvalidDataException($"{nameof(CoreConfigurationDto)}.{nameof(config.GithubPat)} must be non-null.");
    }

    private CoreConfigurationDto? _cachedCoreConfiguration;
}
