﻿using System.Globalization;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git;
using HomeLabManager.Common.Services.Logging;
using LibGit2Sharp;

namespace HomeLabManager.DataTests.Tests;

public sealed class GitDataManagerTests
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        // Delete existing repo (just in case).
        if (Directory.Exists(Utils.TestGitDirectory))
            Utils.DeleteReadOnlyDirectory(Utils.TestGitDirectory);

        // Delete all existing branchess.
        CreateTestData();

        var repoPath = Repository.Init(Utils.TestGitDirectory);
        Assert.That(repoPath, Is.Not.Null);

        var coreConfigurationManager = Utils.CreateCoreConfigurationManager(true).manager;
        var config = coreConfigurationManager.GetActiveCoreConfiguration();

        using (var repo = new Repository(repoPath))
        {
            var remote = repo.Network.Remotes.Add("origin", Utils.GetTestRepoUrl());
            var logManager = new LogManager(true).CreateContextualizedLogManager<GitDataManager>();
            Commands.Fetch(repo, remote.Name, Array.Empty<string>(), CreateFetchOptions(config.GithubUserName, config.GithubPat, logManager), "log");

            var branches = repo.Branches;
            foreach (var branch in branches)
            {
                repo.Network.Push(remote, $"+:{branch.UpstreamBranchCanonicalName}", CreatePushOptions(config.GithubUserName, config.GithubPat, logManager));
            }
        }

        Utils.DeleteReadOnlyDirectory(Utils.TestGitDirectory);
    }

    [SetUp]
    public void SetUp()
    {
        CreateTestData();

        _coreConfigurationManager = Utils.CreateCoreConfigurationManager(true).manager;
        var config = _coreConfigurationManager.GetActiveCoreConfiguration();

        var repoPath = Repository.Init(Utils.TestGitDirectory);
        Assert.That(repoPath, Is.Not.Null);

        using var repo = new Repository(repoPath);
        var logManager = new LogManager(true).CreateContextualizedLogManager<GitDataManager>();
        repo.Commit("Initial empty commit", GitDataManager.CreateGitSignature(config.GitConfigFilePath, logManager), GitDataManager.CreateGitSignature(config.GitConfigFilePath, logManager), new CommitOptions { AllowEmptyCommit = true });

        var testRemote = repo.Network.Remotes.Add("origin", Utils.GetTestRepoUrl());
        _testBranchName = $"test-branch_{DateTime.Now.ToString("MM-dd-yyyy_hh-mm-ss", CultureInfo.CurrentCulture)}";
        var testBranch = repo.CreateBranch(_testBranchName);

        repo.Branches.Update(testBranch,
            b => b.Remote = testRemote.Name,
            b => b.UpstreamBranch = testBranch.CanonicalName
        );
        repo.Network.Push(testBranch, CreatePushOptions(config.GithubUserName, config.GithubPat, logManager));

        Commands.Checkout(repo, testBranch);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(Utils.TestDirectory, true);
        Utils.DeleteReadOnlyDirectory(Utils.TestGitDirectory);
    }

    [Test]
    public void IsDataPathARepo_InvalidRepoDir_ConfirmBehaviorIfRepoDirInvalid()
    {
        void UpdateCoreConfig(string path)
        {
            var coreConfig = _coreConfigurationManager.GetActiveCoreConfiguration();
            coreConfig = coreConfig with { HomeLabRepoDataPath = path };
            _coreConfigurationManager.SaveCoreConfiguration(coreConfig);
        }

        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));

        UpdateCoreConfig(null);
        Assert.That(gitManager.IsDataPathARepo(), Is.False);

        UpdateCoreConfig("");
        Assert.That(gitManager.IsDataPathARepo(), Is.False);

        UpdateCoreConfig("    ");
        Assert.That(gitManager.IsDataPathARepo(), Is.False);

        UpdateCoreConfig(@"C:\doesnt_exist");
        Assert.That(gitManager.IsDataPathARepo(), Is.False);

        UpdateCoreConfig(@"C:\Windows");
        Assert.That(gitManager.IsDataPathARepo(), Is.False);
    }

    [Test]
    public void RepoHasUncommitedChanges_HasChanges_RepoHasUncommitedChanges()
    {
        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));

        Assert.That(gitManager.RepoHasUncommitedChanges(), Is.False);

        File.WriteAllText(Path.Combine(Utils.TestGitDirectory, "testFile.txt"), "Hello World!");

        Assert.That(gitManager.RepoHasUncommitedChanges(), Is.True);
    }

    [Test]
    public void GetRepoStatus_HasNoChanges_RepoHasNoChanges()
    {
        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));

        Assert.That(gitManager.GetRepoStatus().IsDirty, Is.False);
    }

    [Test]
    public void GetRepoStatus_HasNewFile_RepoHasANewUnstagedFile()
    {
        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));

        File.WriteAllText(Path.Combine(Utils.TestGitDirectory, "testFile.txt"), "Hello World!");

        Assert.That(gitManager.GetRepoStatus().IsDirty, Is.True);
        Assert.That(gitManager.GetRepoStatus().Untracked.Count(), Is.EqualTo(1));
    }

    [Test]
    public void PullLatestChanges_HasChangesToPull_PullLatestChangesWithChangesOnRemote()
    {
        var coreConfig = _coreConfigurationManager.GetActiveCoreConfiguration();

        // Push some testing changes.
        var tempRepoDirectory = Path.Combine(Path.GetTempPath(), "gitdatamanager_tests");
        var tempFilePath = Path.Combine(tempRepoDirectory, Path.GetRandomFileName());
        Directory.CreateDirectory(tempRepoDirectory);
        Repository.Init(tempRepoDirectory);

        using (var tempRepo = new Repository(tempRepoDirectory))
        {
            var testRemote = tempRepo.Network.Remotes.FirstOrDefault(Remote => Remote.Name == "origin") ?? tempRepo.Network.Remotes.Add("origin", Utils.GetTestRepoUrl());
            var logManager = new LogManager(true).CreateContextualizedLogManager<GitDataManager>();
            Commands.Fetch(tempRepo, "origin", Array.Empty<string>(), CreateFetchOptions(coreConfig.GithubUserName, coreConfig.GithubPat, logManager), "log");

            var trackedBranch = tempRepo.Branches.First(branch => branch.FriendlyName.Contains(_testBranchName, StringComparison.InvariantCultureIgnoreCase));
            var localBranch = tempRepo.CreateBranch(_testBranchName, trackedBranch.Tip);
            localBranch = tempRepo.Branches.Update(localBranch, b => b.TrackedBranch = trackedBranch.CanonicalName);

            Commands.Checkout(tempRepo, localBranch);

            File.WriteAllText(tempFilePath, "Test Content");
            Commands.Stage(tempRepo, "*");
            tempRepo.Commit("Test file commit", GitDataManager.CreateGitSignature(_coreConfigurationManager.GetActiveCoreConfiguration().GitConfigFilePath, logManager), GitDataManager.CreateGitSignature(_coreConfigurationManager.GetActiveCoreConfiguration().GitConfigFilePath, logManager));

            tempRepo.Network.Push(localBranch, CreatePushOptions(coreConfig.GithubUserName, coreConfig.GithubPat, logManager));
        }
        Utils.DeleteReadOnlyDirectory(tempRepoDirectory);

        var repoFilePath = Path.Combine(Utils.TestGitDirectory, Path.GetFileName(tempFilePath));
        Assert.That(File.Exists(repoFilePath), Is.False);
        
        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));
        gitManager.PullLatestChanges();
        Assert.That(File.Exists(repoFilePath), Is.True);
    }

    [Test]
    public void CommitAndPushChanges_ChangesArePresent_CommitAndPushUncommitedChanges()
    {
        var coreConfig = _coreConfigurationManager.GetActiveCoreConfiguration();
        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));

        File.WriteAllText(Path.Combine(Utils.TestGitDirectory, "testFile.txt"), "Hello World!");

        var commitMessage = "Test commit message";
        var result = gitManager.CommitAndPushChanges(commitMessage);

        Assert.That(result, Is.True);

        // Confirm the changes made it up.
        var tempRepoDirectory = Path.Combine(Path.GetTempPath(), "gitdatamanager_tests");
        Directory.CreateDirectory(tempRepoDirectory);
        Repository.Init(tempRepoDirectory);

        using (var tempRepo = new Repository(tempRepoDirectory))
        {
            var testRemote = tempRepo.Network.Remotes.FirstOrDefault(Remote => Remote.Name == "origin") ?? tempRepo.Network.Remotes.Add("origin", Utils.GetTestRepoUrl());
            var logManager = new LogManager(true).CreateContextualizedLogManager<GitDataManager>();
            Commands.Fetch(tempRepo, "origin", Array.Empty<string>(), CreateFetchOptions(coreConfig.GithubUserName, coreConfig.GithubPat, logManager), "log");

            var trackedBranch = tempRepo.Branches.First(branch => branch.FriendlyName.Contains(_testBranchName, StringComparison.InvariantCultureIgnoreCase));
            var localBranch = tempRepo.CreateBranch(_testBranchName, trackedBranch.Tip);
            localBranch = tempRepo.Branches.Update(localBranch, b => b.TrackedBranch = trackedBranch.CanonicalName);

            Commands.Checkout(tempRepo, localBranch);

            Commands.Pull(tempRepo, GitDataManager.CreateGitSignature(coreConfig.GitConfigFilePath, logManager), CreatePullOptions(coreConfig.GithubUserName, coreConfig.GithubPat, logManager));

            Assert.That(tempRepo.Commits.Any(x => x.MessageShort == commitMessage), Is.True);
            Assert.That(File.Exists(Path.Combine(tempRepoDirectory, "testFile.txt")), Is.True);
        }
        Utils.DeleteReadOnlyDirectory(tempRepoDirectory);
    }

    [Test]
    public void CommitAndPushChanges_NoChanges_NoChangesToCommit()
    {
        var gitManager = new GitDataManager(_coreConfigurationManager, new LogManager(true));

        var commitMessage = "Test commit message";
        var result = gitManager.CommitAndPushChanges(commitMessage);

        Assert.That(result, Is.False);
    }

    private static PushOptions CreatePushOptions(string githubUsername, string githubPat, ContextAwareLogManager<GitDataManager> logManager)
    {
        return new PushOptions
        {
            CredentialsProvider = GitDataManager.CreateGithubCredentialsHandler(githubUsername, githubPat, logManager)
        };
    }

    private static FetchOptions CreateFetchOptions(string githubUsername, string githubPat, ContextAwareLogManager<GitDataManager> logManager)
    {
        return new FetchOptions
        {
            CredentialsProvider = GitDataManager.CreateGithubCredentialsHandler(githubUsername, githubPat, logManager)
        };
    }

    private static PullOptions CreatePullOptions(string githubUsername, string githubPat, ContextAwareLogManager<GitDataManager> logManager)
    {
        return new PullOptions
        {
            FetchOptions = CreateFetchOptions(githubUsername, githubPat, logManager)
        };
    }

    private static void CreateTestData()
    {
        Directory.CreateDirectory(Utils.TestDirectory);
        Directory.CreateDirectory(Utils.TestGitDirectory);
    }

    private CoreConfigurationManager _coreConfigurationManager;
    private string _testBranchName;
}
