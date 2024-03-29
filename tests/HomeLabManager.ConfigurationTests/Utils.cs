﻿using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Services.Logging;

namespace HomeLabManager.DataTests
{
    internal static class Utils
    {
        public const string TestDirectory = "./CoreConfig";
        public const string TestGitDirectory = "./Git";
        public const string TestGitConfigFilePath = "../../../../../TestData/.gitconfig";

        public static (CoreConfigurationManager manager, CoreConfigurationDto coreConfig) CreateCoreConfigurationManager(bool disableCaching)
        {
            var manager = new CoreConfigurationManager(TestDirectory, new LogManager(true));
            var coreConfig = manager.GetOrCreateActiveCoreConfiguration(() => new CoreConfigurationDto
            {
                Name = "Default",
                HomeLabRepoDataPath = TestGitDirectory,
                GitConfigFilePath = TestGitConfigFilePath,
                GithubUserName = File.ReadAllText(GithubUserNameFilePath),
                GithubPat = File.ReadAllText(GithubUserPatFilePath)
            });

            return (manager, coreConfig);
        }

        /// <summary>
        /// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
        /// </summary>
        /// <param name="directory">The name of the directory to remove.</param>
        /// <remarks>From https://stackoverflow.com/questions/25549589/programmatically-delete-local-repository-with-libgit2sharp</remarks>
        public static void DeleteReadOnlyDirectory(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteReadOnlyDirectory(subdirectory);
            }
            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName);
                fileInfo.Attributes = FileAttributes.Normal;
                fileInfo.Delete();
            }
            Directory.Delete(directory);
        }

        public static string GetTestRepoUrl() => File.ReadAllText(GithubRepoUrlFilePath);

        private const string GithubUserNameFilePath = "../../../../../TestData/github_username.txt";
        private const string GithubUserPatFilePath = "../../../../../TestData/github_pat.txt";
        private const string GithubRepoUrlFilePath = "../../../../../TestData/github_repo.txt";
    }
}
