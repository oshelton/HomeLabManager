using System.Diagnostics;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using HomeLabManager.Common.Data.CoreConfiguration;

[assembly: InternalsVisibleTo("HomeLabManager.ManagerTests")]

namespace HomeLabManager.Manager.DesignModeServices;

/// <summary>
/// CoreConfigurationManager to be used at design time.
/// </summary>
internal sealed class DesignCoreConfigurationManager : ICoreConfigurationManager
{
    public string CoreConfigPath => "";

    public bool DisableConfigurationCaching 
    { 
        get => false; 
        set => Debug.WriteLine("Does nothing."); 
    }

    public CoreConfigurationDto GetCoreConfiguration() => s_staticConfiguration;

    public CoreConfigurationDto GetOrCreateCoreConfiguration(Func<CoreConfigurationDto> defaultGenerator) => s_staticConfiguration;

    public void SaveCoreConfiguration(CoreConfigurationDto updatedConfiguration) { }

    public Subject<CoreConfigurationDto> CoreConfigurationUpdated { get; } = new Subject<CoreConfigurationDto>();

    private static readonly CoreConfigurationDto s_staticConfiguration = new() 
    { 
        GitConfigFilePath = "", 
        GithubPat = "", 
        GithubUserName = "", 
        HomeLabRepoDataPath = "C:\\tmp",
    };
}
