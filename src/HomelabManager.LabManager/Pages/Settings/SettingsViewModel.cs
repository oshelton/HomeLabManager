using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Threading;
using Docker.DotNet.Models;
using DynamicData;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Services;
using HomeLabManager.Common.Services.Logging;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Services.SharedDialogs;
using HomeLabManager.Manager.Utils;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace HomeLabManager.Manager.Pages.Settings;

/// <summary>
/// Settings Page View Model.
/// </summary>
public sealed class SettingsViewModel : ValidatedPageBaseViewModel<SettingsViewModel>
{
    public SettingsViewModel() : base()
    {
        _coreConfigurationManager = Program.ServiceProvider.Services.GetService<ICoreConfigurationManager>();
        _navigationService = Program.ServiceProvider.Services.GetService<INavigationService>();
        _sharedDialogService = Program.ServiceProvider.Services.GetService<ISharedDialogsService>();

        _disposables = new CompositeDisposable();

        // Handle switching the currently selected configuration.
        this.WhenAnyValue(x => x.CurrentCoreConfigurationName)
            .Skip(1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(x => !_isResettingCurrentConfiguration && x != null)
            .Subscribe(async x =>
            {
                var coreConfig = _coreConfigurationManager.GetCoreConfiguration(x);
                HandleSelectedConfigurationChanged(coreConfig);
            }).DisposeWith(_disposables);

        SaveCommand = ReactiveCommand.CreateFromTask(Save, this.WhenAnyValue(x => x.Fields.CanSave).ObserveOn(RxApp.MainThreadScheduler))
            .DisposeWith(_disposables);
        SaveCommand.IsExecuting.ToProperty(this, nameof(IsSaving), out _isSaving);

        ResetCommand = ReactiveCommand.CreateFromTask(Reset, this.WhenAnyValue(x => x.Fields.HasChanges).ObserveOn(RxApp.MainThreadScheduler))
            .DisposeWith(_disposables);

        MakeCurrentConfigurationActiveCommand = ReactiveCommand.Create(() => Fields.IsActive = true, outputScheduler: RxApp.MainThreadScheduler)
            .DisposeWith(_disposables);

        DeleteConfigurationCommand = ReactiveCommand.CreateFromTask(DeleteConfiguration);
    }

    public override string Title => "Settings";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not SettingsNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

        LogManager.GetApplicationLogger().Information("Loading configuration settings");

        _allConfigurationInfos = new List<(string Name, bool IsActive)>(_coreConfigurationManager.GetAllCoreConfigurations())
            .OrderBy(x => x.Name).ToArray();
        AllConfigurationNames = _allConfigurationInfos.Select(x => x.Name).ToArray();

        var activeConfigurationInfo = _allConfigurationInfos.First(x => x.IsActive);
        CurrentCoreConfigurationName = activeConfigurationInfo.Name;
    }

    public override async Task<bool> TryNavigateAway()
    {
        var logger = LogManager.GetApplicationLogger();
        if (!Fields.HasChanges || IsSaving)
        {
            return true;
        }
        else
        {
            logger.Information("Attempting to leave page with unsaved changes");
            return  await _sharedDialogService.ShowSimpleYesNoDialog("Unsaved changes will be lost if you continue.").ConfigureAwait(false);
        }
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    public ReactiveCommand<Unit, bool> MakeCurrentConfigurationActiveCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteConfigurationCommand { get; }

    /// <summary>
    /// Whether or not this page is currently saving data.
    /// </summary>
    public bool IsSaving => _isSaving.Value;

    /// <summary>
    /// Names of all available configurations.
    /// </summary>
    public IReadOnlyList<string> AllConfigurationNames
    {
        get => _allConfigurationNames;
        private set => this.RaiseAndSetIfChanged(ref _allConfigurationNames, value);
    }

    /// <summary>
    /// Name of the currently selected configuration.
    /// </summary>
    public string CurrentCoreConfigurationName
    {
        get => _currentCoreConfigurationName;
        set
        {
            if (value == _currentCoreConfigurationName)
                return;

            if (Fields?.HasChanges ?? false)
            {
                async Task VerifyChangeDesired()
                {
                    var shouldContinue = await _sharedDialogService.ShowSimpleYesNoDialog("Unsaved changes will be lost if you continue.").ConfigureAwait(true);

                    if (shouldContinue)
                    {
                        this.RaiseAndSetIfChanged(ref _currentCoreConfigurationName, value);
                    }
                    else
                    {
                        _isResettingCurrentConfiguration = true;

                        var tmp = _currentCoreConfigurationName;
                        _currentCoreConfigurationName = null;
                        await DispatcherHelper.InvokeAsync(() => this.RaisePropertyChanged(nameof(CurrentCoreConfigurationName)), DispatcherPriority.Input).ConfigureAwait(false);
                        _currentCoreConfigurationName = tmp;
                        await DispatcherHelper.InvokeAsync(() => this.RaisePropertyChanged(nameof(CurrentCoreConfigurationName)), DispatcherPriority.Input).ConfigureAwait(false);

                        _isResettingCurrentConfiguration = false;
                    }
                }

                _ = VerifyChangeDesired();
            }
            else
            {
                this.RaiseAndSetIfChanged(ref _currentCoreConfigurationName, value);
            }
        }
    }

    /// <summary>
    /// View Model backing the actual fields.
    /// </summary>
    public SettingsFieldsViewModel Fields
    {
        get => _fields;
        private set => this.RaiseAndSetIfChanged(ref _fields, value);
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
            _disposables.Dispose();
    }

    /// <summary>
    /// Handle updating the Page's state when changing the selected Core Configuration.
    /// </summary>
    /// <param name="config">The configuration being changed to, must not be null.</param>
    private void HandleSelectedConfigurationChanged(CoreConfigurationDto config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        _currentCoreConfiguration = config;
        Fields = new SettingsFieldsViewModel(config, _allConfigurationNames.Except(new[] { CurrentCoreConfigurationName }).ToArray());
    }

    /// <summary>
    /// Save any pending changes to the currently selected core configuration.
    /// </summary>
    private async Task Save()
    {
        if (!Fields.HasChanges)
            throw new InvalidOperationException($"Core configuration {CurrentCoreConfigurationName} has no changes but we are trying to save anyway.");

        var (dialog, dialogTask) = _sharedDialogService.ShowSimpleSavingDataDialog("Saving Core Configuration Changes...");

        LogManager.GetApplicationLogger().Information("Saving updated core configuration settings");

        await Task.Run(() =>
        {
            var mergedResult = Fields.MergeInChanges(_currentCoreConfiguration);
            _coreConfigurationManager!.SaveCoreConfiguration(mergedResult);
            _currentCoreConfiguration = mergedResult;
        }).ConfigureAwait(true);

        Fields = new SettingsFieldsViewModel(_currentCoreConfiguration, _allConfigurationNames.Except(new[] { CurrentCoreConfigurationName }).ToArray());
        AllConfigurationNames = _coreConfigurationManager.GetAllCoreConfigurations().Select(x => x.Name).ToArray();
        CurrentCoreConfigurationName = _currentCoreConfiguration.Name;

        dialog?.GetWindow().Close();
    }

    /// <summary>
    /// Reset the current configuration back to it's initial state.
    /// </summary>
    private async Task Reset()
    {
        if (!Fields.HasChanges)
            throw new InvalidOperationException($"Core configuration {CurrentCoreConfigurationName} has no changes but we are trying to revert anyway.");

        await _sharedDialogService.ShowSimpleYesNoDialog("This will revert any changes you have made.").ConfigureAwait(true);

        Fields = new SettingsFieldsViewModel(_currentCoreConfiguration, _allConfigurationNames.Except(new[] { CurrentCoreConfigurationName }).ToArray());
    }

    /// <summary>
    /// Delete the currently selected configuration.
    /// </summary>
    /// <remarks>Only inactive configurations may be deleted.</remarks>
    private async Task DeleteConfiguration()
    {
        var continueDeleting = await _sharedDialogService.ShowSimpleYesNoDialog("Are you sure you want to delete this configuration?\n\nThis operation cannot be undone.").ConfigureAwait(true);
        if (!continueDeleting)
            return;

        var (dialog, dialogTask) = _sharedDialogService.ShowSimpleSavingDataDialog("Deleting the current core configuration...");

        IReadOnlyList<string> updatedConfigurationList = null;
        CoreConfigurationDto activeCoreConfiguration = null;

        await Task.Run(() =>
        {
            _coreConfigurationManager.DeleteCoreConfiguration(_currentCoreConfiguration);
            updatedConfigurationList = _coreConfigurationManager.GetAllCoreConfigurations().Select(x => x.Name).ToArray();
            activeCoreConfiguration = _coreConfigurationManager.GetActiveCoreConfiguration();
        }).ConfigureAwait(true);

        AllConfigurationNames = updatedConfigurationList;
        CurrentCoreConfigurationName = activeCoreConfiguration.Name;

        dialog?.GetWindow()?.Close();
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly INavigationService _navigationService;
    private readonly ISharedDialogsService _sharedDialogService;

    private readonly CompositeDisposable _disposables;
    private readonly ObservableAsPropertyHelper<bool> _isSaving;

    private IReadOnlyList<(string Name, bool IsActive)> _allConfigurationInfos;
    private IReadOnlyList<string> _allConfigurationNames;
    private CoreConfigurationDto _currentCoreConfiguration;
    private string _currentCoreConfigurationName;
    private SettingsFieldsViewModel _fields;
    private bool _isResettingCurrentConfiguration;
}
