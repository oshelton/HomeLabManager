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
        public CreateEditServerNavigationRequest(bool isNew, BaseServerDto server, int? afterIndex = null) 
        {
            IsNew = isNew;
            Server = server ?? throw new ArgumentNullException(nameof(server));
            AfterIndex = afterIndex;
        }

        /// <summary>
        /// Create instance of the Create/Edit server Page.
        /// </summary>
        public PageBaseViewModel CreatePage() => new CreateEditServerViewModel();

        public bool IsNew { get; }
        public BaseServerDto Server { get; }
        public int? AfterIndex { get; }
    }
}
