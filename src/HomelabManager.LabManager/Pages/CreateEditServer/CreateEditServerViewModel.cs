using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.CreateEditServer;

/// <summary>
/// Server Listing Page View Model.
/// </summary>
public sealed class CreateEditServerViewModel : PageBaseViewModel
{
    public CreateEditServerViewModel(bool isNew, BaseServerDto server)
    {
        if (server is null)
            throw new ArgumentNullException(nameof(server));

        _isNew = isNew;
        _isEditingServerHost = server is ServerHostDto;
        _initialServerTitle = !_isNew ? server.Metadata.DisplayName : null;
    }

    public override string Title => _isNew 
        ? $"Create New {(_isEditingServerHost ? "Server Host" : "Virtual Machine")}"
        : $"Editing {_initialServerTitle}";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not ServerListingNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");
    }

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    private readonly bool _isNew;
    private readonly bool _isEditingServerHost;
    private readonly string _initialServerTitle;
}
