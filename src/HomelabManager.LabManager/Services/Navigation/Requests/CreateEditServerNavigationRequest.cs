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
        public CreateEditServerNavigationRequest(bool isNew, BaseServerDto server) 
        {
            _isNew = isNew;
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        /// <summary>
        /// Create instance of the Create/Edit server Page.
        /// </summary>
        public PageBaseViewModel CreatePage() => new CreateEditServerViewModel(_isNew, _server);

        private bool _isNew;
        private BaseServerDto _server;
    }
}
