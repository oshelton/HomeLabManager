using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia.Threading;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.CreateEditServer.Sections;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.CreateEditServer;

/// <summary>
/// Create/Edit Server Page View Model.
/// </summary>
public sealed class CreateEditServerViewModel : ValidatedPageBaseViewModel
{
    /// <summary>
    /// Design time constructor.
    /// </summary>
    public CreateEditServerViewModel()
    {
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();
    }

    /// Title of the page, a bit more logic trhan usual is used here since this page can be  abit more complicated than most.
    public override string Title => _isNew 
        ? $"Create New {(_isEditingServerHost ? "Server Host" : "Virtual Machine")}"
        : $"Editing {_initialServerTitle}";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not CreateEditServerNavigationRequest realRequest)
            throw new InvalidOperationException("Expected navigation request type is CreateEditServerNavigationRequest.");

        HasChanges = false;
        _isNew = realRequest.IsNew;
        _isEditingServerHost = realRequest.Server is ServerHostDto;
        _initialServerTitle = !_isNew ? realRequest.Server.Metadata.DisplayName : null;

        Metadata = new MetadataEditViewModel(realRequest.Server, realRequest.AllOtherDisplayNames, realRequest.AllOtherNames);

        // Set up an observable to check when content has actually changed.
        _hasChangesSubscription = this.WhenAnyValue(x => x.Metadata.HasChanges)
            .Throttle(TimeSpan.FromSeconds(0.5))
            .Subscribe(hasMetadataChanges => HasChanges = hasMetadataChanges);
    }

    public override Task<bool> TryNavigateAway()
    {
        _hasChangesSubscription?.Dispose();
        Metadata.Dispose();

        return Task.FromResult(true);
    }

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

    public MetadataEditViewModel Metadata
    {
        get => _metadata;
        private set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }

    /// Whether or not this page has changes.
    public bool HasChanges
    {
        get => _hasChanges;
        private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
    }

    /// Whether or not this page is currently saving data.
    public bool IsSaving
    {
        get => _isSaving;
        private set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }

    private readonly IServerDataManager _serverDataManager;
    private readonly INavigationService _navigationService;

    private MetadataEditViewModel _metadata;

    private bool _hasChanges;
    private bool _isSaving;

    private bool _isNew;
    private bool _isEditingServerHost;
    private string _initialServerTitle;

    private IDisposable _hasChangesSubscription;
}
