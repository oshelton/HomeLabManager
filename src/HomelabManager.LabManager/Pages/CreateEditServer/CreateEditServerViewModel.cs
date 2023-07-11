using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
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
public sealed class CreateEditServerViewModel : PageBaseViewModel
{
    /// <summary>
    /// Design time constructor.
    /// </summary>
    public CreateEditServerViewModel()
    {
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();

        _disposables = new CompositeDisposable();

        // Set up an observable to check when content has actually changed and there are no errors.
        _canSave = this.WhenAnyValue(x => x.Metadata.HasChanges, x => x.Metadata.HasErrors,
            (hasMetadataChanges, hasMetadataErrors) => hasMetadataChanges && !hasMetadataErrors)
            .Throttle(TimeSpan.FromSeconds(0.5))
            .ToProperty(this, nameof(CanSave))
            .DisposeWith(_disposables);

        // Set up observable to monitor for validation issues.
        _hasErrors = this.WhenAnyValue(x => x.Metadata.HasErrors)
            .Throttle(TimeSpan.FromSeconds(0.5))
            .ToProperty(this, nameof(HasErrors))
            .DisposeWith(_disposables);

        Save = ReactiveCommand.CreateFromTask(SaveChangesAndNavigateBack, this.WhenAnyValue(x => x.CanSave).ObserveOn(RxApp.MainThreadScheduler))
            .DisposeWith(_disposables);
        Save.IsExecuting.ToProperty(this, nameof(IsSaving), out _isSaving);
    }

    /// Title of the page, a bit more logic trhan usual is used here since this page can be  abit more complicated than most.
    public override string Title => _isNew 
        ? $"Create New {(_isEditingServerHost ? "Server Host" : "Virtual Machine")}"
        : $"Editing {_initialServerTitle}";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not CreateEditServerNavigationRequest realRequest)
            throw new InvalidOperationException("Expected navigation request type is CreateEditServerNavigationRequest.");

        _isNew = realRequest.IsNew;
        _isEditingServerHost = realRequest.Server is ServerHostDto;
        _initialServerTitle = !_isNew ? realRequest.Server.Metadata.DisplayName : null;

        Metadata = new MetadataEditViewModel(realRequest.Server, realRequest.AllOtherDisplayNames, realRequest.AllOtherNames)
            .DisposeWith(_disposables);
    }

    public override Task<bool> TryNavigateAway()
    {
        return Task.FromResult(true);
    }

    public ReactiveCommand<Unit, Unit> Save { get; private set; }

    public INavigationService NavigationService => _navigationService;

    public MetadataEditViewModel Metadata
    {
        get => _metadata;
        private set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }

    /// Whether or not this page has changes.
    public bool CanSave => _canSave.Value;

    /// Whether or not this page has any validation errors.
    public bool HasErrors => _hasErrors.Value;

    /// Whether or not this page is currently saving data.
    public bool IsSaving => _isSaving.Value;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
            _disposables.Dispose();
    }

    private async Task SaveChangesAndNavigateBack()
    {
        var (dialog, dialogTask) = SharedDialogs.ShowSimpleSavingDataDialog("Saving Core Configuration Changes...");

        // Do the actual saving work.
        //await Task.Run(() => ).ConfigureAwait(true);

        dialog.GetWindow().Close();

        await _navigationService.NavigateBack().ConfigureAwait(false);
    }

    private readonly IServerDataManager _serverDataManager;
    private readonly INavigationService _navigationService;
    
    private readonly CompositeDisposable _disposables;
    private readonly ObservableAsPropertyHelper<bool> _canSave;
    private readonly ObservableAsPropertyHelper<bool> _hasErrors;
    private readonly ObservableAsPropertyHelper<bool> _isSaving;

    // Property backing fields.
    private MetadataEditViewModel _metadata;

    // Set once and read internally only fields.
    private bool _isNew;
    private bool _isEditingServerHost;
    private string _initialServerTitle;
}
