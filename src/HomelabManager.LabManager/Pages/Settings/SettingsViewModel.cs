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

        var builder = new ValidationBuilder<SettingsViewModel>();

        builder.RuleFor(vm => vm.HomeLabRepoDataPath)
            .WithPropertyCascadeMode(CascadeMode.Stop)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty the application cannot work as expected.")
            .Must(value => Directory.Exists(value), ValidationMessageType.Error).WithMessage("This should point to a directory.").Throttle(200)
            .Must(value => Repository.IsValid(value), ValidationMessageType.Error).WithMessage("This is not a Git Working Copy; some features will not work properly.").Throttle(1000);

        builder.RuleFor(vm => vm.GitConfigFilePath)
            .WithPropertyCascadeMode(CascadeMode.Stop)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty change tracking will not be able to work as expected.")
            .Must(value => File.Exists(value), ValidationMessageType.Error).WithMessage("This must point to a file that exists.").Throttle(200);

        builder.RuleFor(vm => vm.GithubUserName)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty the application will be unable to push changes to GitHub.");

        builder.RuleFor(vm => vm.GithubPat)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty the application will be unable to push changes to GitHub.");

        Validator = builder.Build(this);

        // Set up observable to monitor for validation issues.
        _hasChanges = this.WhenAnyValue(x => x.CurrentConfigurationIsActive, x => x.HomeLabRepoDataPath, x => x.GitConfigFilePath, x => x.GithubUserName, x => x.GithubPat,
            (isActive, repoPath, gitPath, userName, pat) =>
            {
                return new TrackedPropertyState()
                {
                    IsActive = isActive,
                    HomeLabRepoDataPath = repoPath,
                    GitConfigFilePath = gitPath,
                    GithubUserName = userName,
                    GithubPat = pat
                };
            })
            .Throttle(TimeSpan.FromSeconds(0.5))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(state => !state.Equals(this._initialState))
            .ToProperty(this, nameof(HasChanges))
            .DisposeWith(_disposables);

        // Handle switching the currently selected configuration.
        this.WhenAnyValue(x => x.CurrentCoreConfigurationName)
            .Skip(1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(_ => !_isResettingCurrentConfiguration)
            .Subscribe(async x =>
            {
                var coreConfig = _coreConfigurationManager.GetCoreConfiguration(x);
                HandleSelectedConfigurationChanged(coreConfig);
            }).DisposeWith(_disposables);

        // Set up an observable to check when content has actually changed and there are no errors.
        _canSave = this.WhenAnyValue(x => x.HasErrors, x => x.HasChanges,
            (hasErrors, hasChanges) => !hasErrors && hasChanges)
            .ToProperty(this, nameof(CanSave))
            .DisposeWith(_disposables);

        SaveCommand = ReactiveCommand.CreateFromTask(Save, this.WhenAnyValue(x => x.CanSave).ObserveOn(RxApp.MainThreadScheduler))
            .DisposeWith(_disposables);
        SaveCommand.IsExecuting.ToProperty(this, nameof(IsSaving), out _isSaving);

        CancelCommand = ReactiveCommand.CreateFromTask(_navigationService.NavigateBack)
            .DisposeWith(_disposables);

        MakeCurrentConfigurationActiveCommand = ReactiveCommand.Create(() => CurrentConfigurationIsActive = true, outputScheduler: RxApp.MainThreadScheduler)
            .DisposeWith(_disposables);
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
        if (!HasChanges || IsSaving)
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

    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public ReactiveCommand<Unit, bool> MakeCurrentConfigurationActiveCommand { get; }

    /// Whether or not this page has changes and is in a valid state to be saved.
    public bool CanSave => _canSave.Value;

    /// Whether or not this page has changes, regardless of whether or not they are valid.
    public bool HasChanges => _hasChanges.Value;

    /// Whether or not this page is currently saving data.
    public bool IsSaving => _isSaving.Value;

    public IReadOnlyList<string> AllConfigurationNames
    {
        get => _allConfigurationNames;
        private set => this.RaiseAndSetIfChanged(ref _allConfigurationNames, value);
    }

    public string CurrentCoreConfigurationName
    {
        get => _currentCoreConfigurationName;
        set
        {
            if (value == _currentCoreConfigurationName)
                return;

            if (HasChanges)
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

    public bool CurrentConfigurationIsActive
    {
        get => _currentConfigurationIsActive;
        set => this.RaiseAndSetIfChanged(ref _currentConfigurationIsActive, value);
    }

    public string HomeLabRepoDataPath
    {
        get => _homeLabRepoDataPath;
        set => this.RaiseAndSetIfChanged(ref _homeLabRepoDataPath, value);
    }

    public string GitConfigFilePath
    {
        get => _gitConfigFilePath;
        set => this.RaiseAndSetIfChanged(ref _gitConfigFilePath, value);
    }

    public string GithubUserName
    {
        get => _githubUserName;
        set => this.RaiseAndSetIfChanged(ref _githubUserName, value);
    }

    public string GithubPat
    {
        get => _githubPat;
        set => this.RaiseAndSetIfChanged(ref _githubPat, value);
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
            _disposables.Dispose();
    }

    /// <summary>
    /// Save the 
    /// </summary>
    private async Task Save()
    {
        var (dialog, dialogTask) = _sharedDialogService.ShowSimpleSavingDataDialog("Saving Core Configuration Changes...");

        LogManager.GetApplicationLogger().Information("Saving updated core configuration settings");

        var didIsActiveChange = CurrentConfigurationIsActive && CurrentConfigurationIsActive != _currentCoreConfiguration.IsActive;

        await Task.Run(() =>
        {
            if (didIsActiveChange)
            {
                var previouslyActiveConfiguration = _coreConfigurationManager.GetActiveCoreConfiguration();
                previouslyActiveConfiguration.IsActive = false;
                _coreConfigurationManager.SaveCoreConfiguration(previouslyActiveConfiguration);
            }

            _coreConfigurationManager!.SaveCoreConfiguration(_currentCoreConfiguration with
            {
                IsActive = CurrentConfigurationIsActive,
                HomeLabRepoDataPath = HomeLabRepoDataPath,
                GitConfigFilePath = GitConfigFilePath,
                GithubUserName = GithubUserName,
                GithubPat = GithubPat
            });
        }).ConfigureAwait(true);

        dialog?.GetWindow().Close();

        await _navigationService!.NavigateBack().ConfigureAwait(false);
    }

    private void HandleSelectedConfigurationChanged(CoreConfigurationDto config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config));

        _currentCoreConfiguration = config;

        HomeLabRepoDataPath = config.HomeLabRepoDataPath;
        CurrentConfigurationIsActive = config.IsActive;
        GitConfigFilePath = config.GitConfigFilePath;
        GithubUserName = config.GithubUserName;
        GithubPat = config.GithubPat;

        // Capture the initial state of the data.
        _initialState = new TrackedPropertyState()
        {
            IsActive = config.IsActive,
            HomeLabRepoDataPath = HomeLabRepoDataPath,
            GitConfigFilePath = GitConfigFilePath,
            GithubUserName = GithubUserName,
            GithubPat = GithubPat
        };
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly INavigationService _navigationService;
    private readonly ISharedDialogsService _sharedDialogService;

    private readonly CompositeDisposable _disposables;
    private readonly ObservableAsPropertyHelper<bool> _canSave;
    private readonly ObservableAsPropertyHelper<bool> _hasChanges;
    private readonly ObservableAsPropertyHelper<bool> _isSaving;

    private IReadOnlyList<(string Name, bool IsActive)> _allConfigurationInfos;
    private IReadOnlyList<string> _allConfigurationNames;
    private CoreConfigurationDto _currentCoreConfiguration;
    private string _currentCoreConfigurationName;
    // private bool _isRenamingConfiguration;
    private bool _isResettingCurrentConfiguration;
    private bool _currentConfigurationIsActive;
    private string _homeLabRepoDataPath;
    private string _gitConfigFilePath;
    private string _githubUserName;
    private string _githubPat;

    private TrackedPropertyState? _initialState;

    private struct TrackedPropertyState
    {
        public bool IsActive;
        public string HomeLabRepoDataPath;
        public string GitConfigFilePath;
        public string GithubUserName;
        public string GithubPat;
    }
}
