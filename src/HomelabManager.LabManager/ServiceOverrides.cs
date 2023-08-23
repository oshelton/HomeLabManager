using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.SharedDialogs;

namespace HomeLabManager.Manager
{
    /// <summary>
    /// Class for providing service overrides for testing purposes.
    /// </summary>
    public sealed class ServiceOverrides
    {
        public Func<ICoreConfigurationManager> CoreConfigurationManagerServiceBuilder { get; init; }
        public Func<IServerDataManager> ServerDataManagerServiceBuilder { get; init; }
        public Func<INavigationService> NavigationServiceBuilder { get; init; }
        public Func<ISharedDialogsService> SharedDialogsServiceBuilder { get; init; }
    }
}
