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
public sealed class CreateEditServerViewModel : ValidatedPageBaseViewModel
{
    // Design time constructor.
    public CreateEditServerViewModel()
    {
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();
    }

    public CreateEditServerViewModel(bool isNew, BaseServerDto server)
        : this()
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
        if (request is not CreateEditServerNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is CreateEditServerNavigationRequest.");
    }

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    public async Task SaveChangesAndNavigateBack()
    {
        IsSaving = true;

        var (dialog, dialogTask) = SharedDialogs.ShowSimpleSavingDataDialog("Saving Core Configuration Changes...");

        // Do the actual saving work.
        //await Task.Run(() => ).ConfigureAwait(true);

        dialog.GetWindow().Close();

        IsSaving = false;
        HasChanges = false;

        await _navigationService!.NavigateBack().ConfigureAwait(false);
    }

    public INavigationService NavigationService => _navigationService;

    public bool HasChanges
    {
        get => _hasChanges;
        private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }

    private readonly IServerDataManager _serverDataManager;
    private readonly INavigationService _navigationService;

    private bool _hasChanges;
    private bool _isSaving;

    private readonly bool _isNew;
    private readonly bool _isEditingServerHost;
    private readonly string _initialServerTitle;
}
