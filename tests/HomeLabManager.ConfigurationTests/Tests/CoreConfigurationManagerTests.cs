using HomeLabManager.Common.Data.CoreConfiguration;

namespace HomeLabManager.DataTests.Tests;

public sealed class CoreConfigurationManagerTests
{
    [SetUp]
    public void SetUp()
    {
        Directory.CreateDirectory(Utils.TestDirectory);
        Directory.CreateDirectory(Utils.TestGitDirectory);
        File.WriteAllText(Utils.TestGitConfigFilePath, @"[user]\n\tname = Owen Shelton\n\temail = jowenshelton@gmail.com");
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(Utils.TestDirectory, true);
        Directory.Delete(Utils.TestGitDirectory, true);
    }

    [Test]
    public void ManagerCreation()
    {
        Assert.DoesNotThrow(() => new CoreConfigurationManager(Utils.TestDirectory));
        Assert.Throws<ArgumentNullException>(() => new CoreConfigurationManager(string.Empty));
        Assert.Throws<InvalidOperationException>(() => new CoreConfigurationManager("./NonExistentDir"));
    }

    [Test]
    public void CoreConfigCreationAndRetrieval()
    {
        var (manager, coreConfig) = Utils.CreateCoreConfigurationManager(true);

        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("owen"));
        Assert.That(coreConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void CoreConfigRetrieval()
    {
        var (manager, _) = Utils.CreateCoreConfigurationManager(true);
        var coreConfig = manager.GetCoreConfiguration();

        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("owen"));
        Assert.That(coreConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void CoreConfigSaving()
    {
        var (manager, coreConfig) = Utils.CreateCoreConfigurationManager(true);

        coreConfig = coreConfig with { GithubUserName = "Owen shelton" };
        manager.SaveCoreConfiguration(coreConfig);

        var newConfig = manager.GetCoreConfiguration();

        Assert.That(newConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(newConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(newConfig.GithubUserName, Is.EqualTo(coreConfig.GithubUserName));
        Assert.That(newConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void CoreConfigCaching()
    {
        var (manager, _) = Utils.CreateCoreConfigurationManager(false);

        File.Delete(manager.CoreConfigPath);

        var coreConfig = manager.GetCoreConfiguration();

        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(Utils.TestGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(Utils.TestGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("owen"));
        Assert.That(coreConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void CoreConfigChangeNotifying()
    {
        var (manager, coreConfig) = Utils.CreateCoreConfigurationManager(true);

        coreConfig = coreConfig with { GithubUserName = "Owen shelton" };

        CoreConfigurationDto? updatedDto = null;
        var subscription = manager.CoreConfigurationUpdated.Subscribe(config => updatedDto = config);

        manager.SaveCoreConfiguration(coreConfig);

        Assert.That(updatedDto, Is.Not.Null);
        Assert.That(updatedDto, Is.EqualTo(coreConfig));
        subscription.Dispose();
    }
}
