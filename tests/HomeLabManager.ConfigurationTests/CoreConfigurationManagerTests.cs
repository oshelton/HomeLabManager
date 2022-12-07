using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.CoreConfiguration;

namespace HomeLabManager.DataTests;

public sealed class CoreConfigurationManagerTests
{
    [SetUp]
    public void SetUp() 
    { 
        Directory.CreateDirectory(_testDirectory);
        Directory.CreateDirectory(_testGitDirectory);
        File.WriteAllText(_testGitConfigFilePath, @"[user]\n\tname = Owen Shelton\n\temail = jowenshelton@gmail.com");
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_testDirectory, true);
    }

    [Test]
    public void TestManagerCreation()
    {
        Assert.DoesNotThrow(() => new CoreConfigurationManager(_testDirectory));
        Assert.Throws<ArgumentNullException>(() => new CoreConfigurationManager(string.Empty));
        Assert.Throws<InvalidOperationException>(() => new CoreConfigurationManager("./NonExistentDir"));
    }

    [Test]
    public void TestCoreConfigCreationAndRetrieval()
    {
        var manager = new CoreConfigurationManager(_testDirectory);
        manager.DisableConfigurationCaching = true;
        var coreConfig = manager.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
        {
            HomeLabRepoDataPath = _testGitDirectory,
            GitConfigFilePath = _testGitConfigFilePath,
            GithubUserName = "owen",
            GithubPat = "pat"
        });

        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(_testGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(_testGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("owen"));
        Assert.That(coreConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void TestCoreConfigRetrieval()
    {
        var manager = new CoreConfigurationManager(_testDirectory);
        manager.DisableConfigurationCaching = true;
        manager.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
        {
            HomeLabRepoDataPath = _testGitDirectory,
            GitConfigFilePath = _testGitConfigFilePath,
            GithubUserName = "owen",
            GithubPat = "pat"
        });
        var coreConfig = manager.GetCoreConfiguration();

        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(_testGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(_testGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("owen"));
        Assert.That(coreConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void TestCoreConfigSaving()
    {
        var manager = new CoreConfigurationManager(_testDirectory);
        manager.DisableConfigurationCaching = true;
        var coreConfig = manager.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
        {
            HomeLabRepoDataPath = _testGitDirectory,
            GitConfigFilePath = _testGitConfigFilePath,
            GithubUserName = "owen",
            GithubPat = "pat"
        });

        coreConfig = coreConfig with { GithubUserName = "Owen shelton" };
        manager.SaveCoreConfiguration(coreConfig);

        var newConfig = manager.GetCoreConfiguration();

        Assert.That(newConfig.HomeLabRepoDataPath, Is.EqualTo(_testGitDirectory));
        Assert.That(newConfig.GitConfigFilePath, Is.EqualTo(_testGitConfigFilePath));
        Assert.That(newConfig.GithubUserName, Is.EqualTo(coreConfig.GithubUserName));
        Assert.That(newConfig.GithubPat, Is.EqualTo("pat"));
    }

    [Test]
    public void TestCoreConfigCaching()
    {
        var manager = new CoreConfigurationManager(_testDirectory);
        manager.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
        {
            HomeLabRepoDataPath = _testGitDirectory,
            GitConfigFilePath = _testGitConfigFilePath,
            GithubUserName = "owen",
            GithubPat = "pat"
        });

        File.Delete(manager.CoreConfigPath);

        var coreConfig = manager.GetCoreConfiguration();

        Assert.That(coreConfig.HomeLabRepoDataPath, Is.EqualTo(_testGitDirectory));
        Assert.That(coreConfig.GitConfigFilePath, Is.EqualTo(_testGitConfigFilePath));
        Assert.That(coreConfig.GithubUserName, Is.EqualTo("owen"));
        Assert.That(coreConfig.GithubPat, Is.EqualTo("pat"));
    }

    private const string _testDirectory = "./CoreConfig";
    private const string _testGitDirectory = "./Git";
    private const string _testGitConfigFilePath = "./.config";
}
