using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Pages.CreateEditServer.Sections;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Services.SharedDialogs;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.CreateEditServer;

/// <summary>
/// Create/Edit Server Page View Model.
/// </summary>
public sealed class CreateEditServerViewModel : PageBaseViewModel<CreateEditServerViewModel>
{
    /// <summary>
    /// Design time constructor.
    /// </summary>
    public CreateEditServerViewModel() : base()
    {
        _serverDataManager = Program.ServiceProvider.Services.GetService<IServerDataManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();
        _sharedDialogsService = Program.ServiceProvider.Services.GetService<ISharedDialogsService>();

        _disposables = new CompositeDisposable();

        // Set up an observable to check when content has actually changed and there are no errors.
        _canSave = this.WhenAnyValue(x => x.Metadata.HasChanges, x => x.Metadata.HasErrors,
            (hasMetadataChanges, hasMetadataErrors) => hasMetadataChanges && !hasMetadataErrors)
            .ToProperty(this, nameof(CanSave))
            .DisposeWith(_disposables);

        // Set up observable to monitor for validation issues.
        _hasChanges = this.WhenAnyValue(x => x.Metadata.HasChanges)
            .ToProperty(this, nameof(HasChanges))
            .DisposeWith(_disposables);

        // Set up observable to monitor for validation issues.
        _hasErrors = this.WhenAnyValue(x => x.Metadata.HasErrors)
            .ToProperty(this, nameof(HasErrors))
            .DisposeWith(_disposables);

        SaveCommand = ReactiveCommand.CreateFromTask(Save, this.WhenAnyValue(x => x.CanSave).ObserveOn(RxApp.MainThreadScheduler))
            .DisposeWith(_disposables);
        SaveCommand.IsExecuting.ToProperty(this, nameof(IsSaving), out _isSaving);

        CancelCommand = ReactiveCommand.CreateFromTask(_navigationService.NavigateBack)
            .DisposeWith(_disposables);
    }

    /// Title of the page, a bit more logic than usual is used here since this page can be  abit more complicated than most.
    public override string Title => _isNew 
        ? $"Create New {(_isEditingServerHost ? "Server Host" : "Virtual Machine")}"
        : $"Editing {_initialServerTitle}";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not CreateEditServerNavigationRequest realRequest)
            throw new InvalidOperationException("Expected navigation request type is CreateEditServerNavigationRequest.");

        var logger = LogManager.GetApplicationLogger();

        _serverDto = realRequest.Server;

        _isNew = realRequest.IsNew;
        _isEditingServerHost = realRequest.Server is ServerHostDto;
        _initialServerTitle = !_isNew ? realRequest.Server.Metadata.DisplayName : null;
        _afterIndex = realRequest.AfterIndex;

        logger.Information("Loading servers");

        IReadOnlyList<ServerHostDto> allServers = null;
        IReadOnlyList<string> allOtherDisplayNames = null;
        IReadOnlyList<string> allOtherNames = null;
        await Task.Run(() =>
        {
            allServers = _serverDataManager.GetServers();
            var flattenedServerList = allServers
                .Where(x => x.UniqueId != _serverDto.UniqueId)
                .Cast<BaseServerDto>()
                .Union(allServers.SelectMany(x => x.VMs).Where(x => x.UniqueId != _serverDto.UniqueId))
                .ToArray();

            allOtherDisplayNames = flattenedServerList.Select(x => x.Metadata.DisplayName).ToArray();
            allOtherNames = flattenedServerList.Select(x => x.Metadata.Name).ToArray();
        }).ConfigureAwait(true);

        logger.Information("Loaded servers");

        Metadata = new MetadataEditViewModel(realRequest.Server, allOtherDisplayNames, allOtherNames)
            .DisposeWith(_disposables);

        if (_isEditingServerHost)
        {
            var host = _serverDto as ServerHostDto;
            _allExistingSiblingServers = allServers.Where(x => x.UniqueId != _serverDto.UniqueId).ToList();
        }
        else
        {
            var vm = _serverDto as ServerVmDto;
            _allExistingSiblingServers = vm.Host.VMs.Where(x => x.UniqueId != _serverDto.UniqueId).ToList();
        }
    }

    public override Task<bool> TryNavigateAway()
    {
        var logger = LogManager.GetApplicationLogger();
        if (!HasChanges || IsSaving)
        {
            logger.Information("Leaving page without having made any changes");
            return Task.FromResult(true);
        }
        else
        {
            logger.Information("Attempting to leave page with unsaved changes");
            return _sharedDialogsService.ShowSimpleYesNoDialog("Unsaved changes will be lost if you continue.");
        }
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public MetadataEditViewModel Metadata
    {
        get => _metadata;
        private set => this.RaiseAndSetIfChanged(ref _metadata, value);
    }

    /// Whether or not this page has changes and is in a valid state to be saved.
    public bool CanSave => _canSave.Value;

    /// Whether or not this page has changes, regardless of whether or not they are valid.
    public bool HasChanges => _hasChanges.Value;

    /// Whether or not this page has any validation errors.
    public bool HasErrors => _hasErrors.Value;

    /// Whether or not this page is currently saving data.
    public bool IsSaving => _isSaving.Value;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
            _disposables.Dispose();
    }

    private async Task Save()
    {
        var (dialog, dialogTask) = _sharedDialogsService.ShowSimpleSavingDataDialog("Saving Core Configuration Changes...");

        // Incorporate updated information into Dto.
        var updatedDto = _serverDto with
        {
            Metadata = _serverDto.Metadata with
            {
                DisplayName = Metadata.DisplayName,
                Name = Metadata.Name,
                DisplayIndex = _afterIndex.HasValue ? _afterIndex.Value + 1 : 0,
            }
        };

        var logger = LogManager.GetApplicationLogger();

        // Do the actual saving work.
        await Task.Run(() => 
        {
            if (_isEditingServerHost)
            {
                logger.Information("Saving server \"{UniqueID}\"", updatedDto.UniqueId);
                _serverDataManager.AddUpdateServer(updatedDto as ServerHostDto);

                if (_isNew)
                {
                    var otherAffectedServers = _allExistingSiblingServers.Where(x => x.Metadata.DisplayIndex >= updatedDto.Metadata.DisplayIndex)
                        .Select(x => x with { Metadata = x.Metadata with { DisplayIndex = x.Metadata.DisplayIndex + 1 } })
                        .Cast<ServerHostDto>()
                        .ToArray();

                    logger.Information("Updating display index of \"{ServerCount}\" servers", otherAffectedServers.Length);

                    foreach (var host in otherAffectedServers)
                    {
                        _serverDataManager.AddUpdateServer(host);
                    }
                }
            }
            else if (updatedDto is ServerVmDto vmDto)
            {
                var host = vmDto.Host;
                if (_isNew)
                {
                    host.VMs = host.VMs.Select(x => x with 
                    { 
                        Metadata = x.Metadata with 
                        { 
                            DisplayIndex = x == vmDto ? x.Metadata.DisplayIndex : (
                                x.Metadata.DisplayIndex < _afterIndex ? x.Metadata.DisplayIndex : x.Metadata.DisplayIndex + 1
                            )
                        } 
                    }).ToList();
                }

                logger.Information("Updating server \"{UniqueID}\" as its child vm \"{VMID}\" has been updated", host.UniqueId, vmDto.UniqueId);
                
                _serverDataManager.AddUpdateServer(host);
            }
        }).ConfigureAwait(true);

        dialog?.GetWindow().Close();

        await _navigationService.NavigateBack().ConfigureAwait(false);
    }

    private readonly IServerDataManager _serverDataManager;
    private readonly INavigationService _navigationService;
    private readonly ISharedDialogsService _sharedDialogsService;
    
    private readonly CompositeDisposable _disposables;
    private readonly ObservableAsPropertyHelper<bool> _canSave;
    private readonly ObservableAsPropertyHelper<bool> _hasChanges;
    private readonly ObservableAsPropertyHelper<bool> _hasErrors;
    private readonly ObservableAsPropertyHelper<bool> _isSaving;

    // Property backing fields.
    private MetadataEditViewModel _metadata;

    // Set once and read internally only fields.
    private BaseServerDto _serverDto;
    private bool _isNew;
    private bool _isEditingServerHost;
    private string _initialServerTitle;
    private int? _afterIndex;
    private IReadOnlyList<BaseServerDto> _allExistingSiblingServers;
}
