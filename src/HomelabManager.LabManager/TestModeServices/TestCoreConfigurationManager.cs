using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using HomeLabManager.Common.Data.CoreConfiguration;

[assembly: InternalsVisibleTo("HomeLabManager.ManagerTests")]

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// CoreConfigurationManager to be used with unit tests..
/// </summary>
internal sealed class TestCoreConfigurationManager : ICoreConfigurationManager
{
    public void Configure(Func<TestCoreConfigurationManager, string>? getCoreConfigPath = null,
        Func<TestCoreConfigurationManager, CoreConfigurationDto>? getCoreConfiguration = null,
        Func<TestCoreConfigurationManager, CoreConfigurationDto>? getOrCreateCoreConfiguration = null,
        Action<TestCoreConfigurationManager>? saveCoreConfiguration = null)
    {
        _getCoreConfigPath = getCoreConfigPath;
        _getCoreConfiguration = getCoreConfiguration;
        _getOrCreateCoreConfiguration = getOrCreateCoreConfiguration;
        _saveCoreConfiguration = saveCoreConfiguration;
    }

    public string CoreConfigPath => _getCoreConfigPath is not null ? _getCoreConfigPath(this) : "";

    public bool DisableConfigurationCaching { get; set; }

    public CoreConfigurationDto GetCoreConfiguration() => _getCoreConfiguration is not null ? _getCoreConfiguration(this) : s_staticConfiguration;

    public CoreConfigurationDto GetOrCreateCoreConfiguration(Func<CoreConfigurationDto> defaultGenerator) => _getOrCreateCoreConfiguration is not null ? _getOrCreateCoreConfiguration(this) : s_staticConfiguration;

    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration)
    {
        if (_saveCoreConfiguration is not null)
            _saveCoreConfiguration(this);
    }

    public Subject<CoreConfigurationDto> CoreConfigurationUpdated { get; } = new Subject<CoreConfigurationDto>();

    private static readonly CoreConfigurationDto s_staticConfiguration = new() 
    { 
        GitConfigFilePath = "", 
        GithubPat = "", 
        GithubUserName = "", 
        HomeLabRepoDataPath = "C:\\tmp",
    };

    private Func<TestCoreConfigurationManager, string>? _getCoreConfigPath;
    private Func<TestCoreConfigurationManager, CoreConfigurationDto>? _getCoreConfiguration;
    private Func<TestCoreConfigurationManager, CoreConfigurationDto>? _getOrCreateCoreConfiguration;
    private Action<TestCoreConfigurationManager>? _saveCoreConfiguration;
}
