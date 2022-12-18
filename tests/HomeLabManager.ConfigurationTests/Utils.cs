using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.CoreConfiguration;

namespace HomeLabManager.DataTests
{
    internal static class Utils
    {
        public const string TestDirectory = "./CoreConfig";
        public const string TestGitDirectory = "./Git";
        public const string TestGitConfigFilePath = "./.config";

        public static (CoreConfigurationManager manager, CoreConfigurationDto coreConfig) CreateCoreConfigurationManager(bool disableCaching)
        {
            var manager = new CoreConfigurationManager(TestDirectory);
            manager.DisableConfigurationCaching = disableCaching;
            var coreConfig = manager.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
            {
                HomeLabRepoDataPath = TestGitDirectory,
                GitConfigFilePath = TestGitConfigFilePath,
                GithubUserName = "owen",
                GithubPat = "pat"
            });

            return (manager, coreConfig);
        }
    }
}
