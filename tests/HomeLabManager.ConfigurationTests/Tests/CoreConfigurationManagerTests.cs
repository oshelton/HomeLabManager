using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Services.Logging;

namespace HomeLabManager.DataTests.Tests;

public sealed class CoreConfigurationManagerTests
{
    [SetUp]
    public void SetUp()
    {
        Directory.CreateDirectory(Utils.TestDirectory);
        Directory.CreateDirectory(Utils.TestGitDirectory);
    }

    [TearDown]
    public async Task TearDown()
    {
        Directory.Delete(Utils.TestDirectory, true);

        await Task.Delay(50).ConfigureAwait(false);
        Directory.Delete(Utils.TestGitDirectory, true);
    }

    [Test]
    public void Constructor_VerifyConstructor_VerifyConstructorHandlesArgumentsAsExpected()
    {
        Assert.DoesNotThrow(() => new CoreConfigurationManager(Utils.TestDirectory, new LogManager(true)));
        Assert.Throws<ArgumentNullException>(() => new CoreConfigurationManager(string.Empty, new LogManager(true)));
        Assert.Throws<InvalidOperationException>(() => new CoreConfigurationManager("./NonExistentDir", new LogManager(true)));
    }

    [Test]
    public void GetOrCreateActiveCoreConfiguration_CoreConfigurationCreated_VerifyActiveCoreConfigurationCreatedAsExpected()
    {
        var (manager, coreConfig) = Utils.CreateCoreConfigurationManager(true);

        Assert.That(coreConfig.Name, Is.EqualTo("Default"));
        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("oshelton"));
        Assert.That(coreConfig.GithubPat, Is.Not.Null);
        Assert.That(coreConfig.IsActive, Is.True);
        Assert.That(coreConfig.InitialName, Is.EqualTo("Default"));
        Assert.That(coreConfig.InitialIsActive, Is.True);
        Assert.That(coreConfig.FilePath, Is.EqualTo(manager.ActiveCoreConfigPath));
    }

    [Test]
    public void GetOrCreateActiveCoreConfiguration_CoreConfigurationRetrieved_VerifyRetrievingExistingConfigurationWorks()
    {
        var (manager, _) = Utils.CreateCoreConfigurationManager(true);
        var coreConfig = manager.GetOrCreateActiveCoreConfiguration(() => null);

        Assert.That(coreConfig.Name, Is.EqualTo("Default"));
        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("oshelton"));
        Assert.That(coreConfig.GithubPat, Is.Not.Null);
        Assert.That(coreConfig.IsActive, Is.True);
        Assert.That(coreConfig.InitialName, Is.EqualTo("Default"));
        Assert.That(coreConfig.InitialIsActive, Is.True);
        Assert.That(coreConfig.FilePath, Is.EqualTo(manager.ActiveCoreConfigPath));
    }

    [Test]
    public void GetActiveCoreConfiguration_CoreConfigurationRetrieved_VerifyRetrievingExistingConfigurationWorks()
    {
        var (manager, _) = Utils.CreateCoreConfigurationManager(true);
        var coreConfig = manager.GetActiveCoreConfiguration();

        Assert.That(coreConfig.Name, Is.EqualTo("Default"));
        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("oshelton"));
        Assert.That(coreConfig.GithubPat, Is.Not.Null);
        Assert.That(coreConfig.IsActive, Is.True);
        Assert.That(coreConfig.InitialName, Is.EqualTo("Default"));
        Assert.That(coreConfig.InitialIsActive, Is.True);
        Assert.That(coreConfig.FilePath, Is.EqualTo(manager.ActiveCoreConfigPath));
    }

    [Test]
    public void GetCoreConfiguration_GetActiveConfiguration_VerifyActiveConfigurationCanBeRetrieved()
    {
        var (manager, defaultConfiguration) = Utils.CreateCoreConfigurationManager(true);

        var otherConfig = manager.GetCoreConfiguration(defaultConfiguration.Name);
        Assert.That(otherConfig, Is.Not.Null);
        Assert.That(otherConfig.Name, Is.EqualTo(defaultConfiguration.Name));
    }

    [Test]
    public void GetCoreConfiguration_GetInactiveConfiguration_VerifyInactiveConfigurationCanBeRetrieved()
    {
        var (manager, _) = Utils.CreateCoreConfigurationManager(true);

        var configTwo = new CoreConfigurationDto() { Name = "OtherConfig" };
        manager.SaveCoreConfiguration(configTwo);

        var otherConfig = manager.GetCoreConfiguration(configTwo.Name);
        Assert.That(otherConfig, Is.Not.Null);
        Assert.That(otherConfig.Name, Is.EqualTo(configTwo.Name));
    }

    [Test]
    public void SaveCoreConfiguration_UpdateDefaultActiveConfig_ModifyAndSaveTheActiveCoreConfig()
    {
        var (manager, coreConfig) = Utils.CreateCoreConfigurationManager(true);

        coreConfig = coreConfig with { GithubUserName = "Owen shelton" };
        manager.SaveCoreConfiguration(coreConfig);

        var newConfig = manager.GetActiveCoreConfiguration();

        Assert.That(newConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(newConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(newConfig.GithubUserName, Is.EqualTo(coreConfig.GithubUserName));
        Assert.That(newConfig.GithubPat, Is.EqualTo(coreConfig.GithubPat));
    }

    [Test]
    public void SaveCoreConfiguration_SaveNewInactiveConfig_ModifyAndSaveANewCoreConfig()
    {
        var (manager, defaultConfiguration) = Utils.CreateCoreConfigurationManager(true);

        var newConfig = new CoreConfigurationDto
        {
            Name = "New Config",
            GithubUserName = "Owen",
            HomeLabRepoDataPath = Path.GetTempPath()
        };
        manager.SaveCoreConfiguration(newConfig);

        var retrieved = manager.GetCoreConfiguration(newConfig.Name);

        Assert.That(retrieved.Name, Is.EqualTo(newConfig.Name));
        Assert.That(retrieved.GithubUserName, Is.EqualTo(newConfig.GithubUserName));
        Assert.That(retrieved.HomeLabRepoDataPath, Is.EqualTo(newConfig.HomeLabRepoDataPath));
        Assert.That(retrieved.InitialName, Is.EqualTo(newConfig.InitialName));
        Assert.That(retrieved.FilePath, Is.EqualTo(newConfig.FilePath));

        newConfig.Name = "New Name";
        manager.SaveCoreConfiguration(newConfig);

        retrieved = manager.GetCoreConfiguration(newConfig.Name);

        Assert.That(retrieved.Name, Is.EqualTo(newConfig.Name));
        Assert.That(retrieved.GithubUserName, Is.EqualTo(newConfig.GithubUserName));
        Assert.That(retrieved.HomeLabRepoDataPath, Is.EqualTo(newConfig.HomeLabRepoDataPath));
        Assert.That(retrieved.InitialName, Is.EqualTo(newConfig.InitialName));
        Assert.That(retrieved.FilePath, Is.EqualTo(newConfig.FilePath));

        var allConfigs = manager.GetAllCoreConfigurations();
        Assert.That(allConfigs.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetAllCoreConfigurations_CoreConfigurationsExpectedExist_AllCoreConfigurationsRetrievedAsExpected()
    {
        var (manager, defaultConfiguration) = Utils.CreateCoreConfigurationManager(true);
        var newConfiguration = new CoreConfigurationDto() { Name = "New 1" };
        manager.SaveCoreConfiguration(newConfiguration);

        var newConfiguration2 = new CoreConfigurationDto() { Name = "New 2" };
        manager.SaveCoreConfiguration(newConfiguration2);

        var allConfigurations = manager.GetAllCoreConfigurations();
        
        Assert.That(allConfigurations.Count, Is.EqualTo(3));
        Assert.That(allConfigurations.Any(x => x.Name == "Default"), Is.True);
        Assert.That(allConfigurations.First(x => x.Name == "Default").IsActive, Is.True);
        Assert.That(allConfigurations.Any(x => x.Name == newConfiguration.Name), Is.True);
        Assert.That(allConfigurations.First(x => x.Name == newConfiguration.Name).IsActive, Is.False);
        Assert.That(allConfigurations.Any(x => x.Name == newConfiguration2.Name), Is.True);
        Assert.That(allConfigurations.First(x => x.Name == newConfiguration2.Name).IsActive, Is.False);
    }

    [Test]
    public void DeleteCoreConfiguration_VerifyErrorConditions_VerifyArgumentExceptionsThrownAsExpected()
    {
        var (manager, defaultConfiguration) = Utils.CreateCoreConfigurationManager(true);
        var newConfiguration = new CoreConfigurationDto() { Name = "Test" };

        Assert.Throws<ArgumentNullException>(() => manager.DeleteCoreConfiguration(null));
        Assert.Throws<InvalidOperationException>(() => manager.DeleteCoreConfiguration(defaultConfiguration));
        Assert.Throws<InvalidOperationException>(() => manager.DeleteCoreConfiguration(newConfiguration));
    }

    [Test]
    public void DeleteCoreConfiguration_DeleteExistingConfiguration_VerifyDeletingConfigurationsWorks()
    {
        var (manager, defaultConfiguration) = Utils.CreateCoreConfigurationManager(true);
        var newConfiguration = new CoreConfigurationDto() { Name = "Test" };
        manager.SaveCoreConfiguration(newConfiguration);

        Assert.That(File.Exists(newConfiguration.FilePath), Is.True);
        manager.DeleteCoreConfiguration(newConfiguration);
        Assert.That(File.Exists(newConfiguration.FilePath), Is.False);
    }

    [Test]
    public void ActiveCoreConfigPath_VerifyActiveCoreConfigRetrievalWorks_VerifyActivePathWorksAsExpected()
    {
        var (manager, defaultConfiguration) = Utils.CreateCoreConfigurationManager(true);

        Assert.That(manager.ActiveCoreConfigPath, Is.EqualTo(defaultConfiguration.FilePath));

        defaultConfiguration.IsActive = false;
        manager.SaveCoreConfiguration(defaultConfiguration);

        Assert.That(manager.ActiveCoreConfigPath, Is.Null);

        var newConfig = new CoreConfigurationDto 
        { 
            Name = "New", 
            IsActive = true 
        };
        manager.SaveCoreConfiguration(newConfig);

        Assert.That(manager.ActiveCoreConfigPath, Is.EqualTo(newConfig.FilePath));

    }

    [Test]
    public void CoreConfigurationUpdated_ConfigChangeNotifications_VerifyConfigChangeNotificationsBroadcastAsExpected()
    {
        var (manager, coreConfig) = Utils.CreateCoreConfigurationManager(true);

        coreConfig = coreConfig with { GithubUserName = "Owen shelton" };

        CoreConfigurationDto updatedDto = null;
        var subscription = manager.CoreConfigurationUpdated.Subscribe(config => updatedDto = config);

        manager.SaveCoreConfiguration(coreConfig);

        Assert.That(updatedDto, Is.Not.Null);
        Assert.That(updatedDto, Is.EqualTo(coreConfig));

        var newConfig = new CoreConfigurationDto { Name = "new config" };
        manager.SaveCoreConfiguration(newConfig);

        Assert.That(updatedDto, Is.Not.Null);
        Assert.That(updatedDto, Is.EqualTo(newConfig));

        subscription.Dispose();
    }
}
