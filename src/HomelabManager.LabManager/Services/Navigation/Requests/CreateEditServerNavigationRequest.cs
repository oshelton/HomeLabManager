using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.CreateEditServer;

namespace HomeLabManager.Manager.Services.Navigation.Requests
{
    /// <summary>
    /// Navigation Request class for the Create/Edit server Page.
    /// </summary>
    public sealed class CreateEditServerNavigationRequest : INavigationRequest
    {
        public CreateEditServerNavigationRequest(bool isNew, BaseServerDto server, IReadOnlyList<string> allOtherDisplayNames, IReadOnlyList<string> allOtherNames) 
        {
            IsNew = isNew;
            Server = server ?? throw new ArgumentNullException(nameof(server));
            AllOtherDisplayNames = allOtherDisplayNames ?? throw new ArgumentNullException(nameof(allOtherDisplayNames));
            AllOtherNames = allOtherNames ?? throw new ArgumentNullException(nameof(allOtherNames));
        }

        /// <summary>
        /// Create instance of the Create/Edit server Page.
        /// </summary>
        public PageBaseViewModel CreatePage() => new CreateEditServerViewModel();

        public bool IsNew { get; private set; }
        public BaseServerDto Server { get; private set; }
        public IReadOnlyList<string> AllOtherDisplayNames { get; private set; }
        public IReadOnlyList<string> AllOtherNames { get; private set; }
    }
}
