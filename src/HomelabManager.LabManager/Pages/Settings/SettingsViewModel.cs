using System.Reactive.Linq;
using Avalonia.Platform.Storage;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Services.SharedDialogs;
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

        var builder = new ValidationBuilder<SettingsViewModel>();

        builder.RuleFor(vm => vm.HomeLabRepoDataPath)
            .WithPropertyCascadeMode(CascadeMode.Stop)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty the application cannot work as expected.")
            .Must(value => Directory.Exists(value), ValidationMessageType.Warning).WithMessage("This should point to a directory.").Throttle(1000)
            .Must(value => Repository.IsValid(value), ValidationMessageType.Warning).WithMessage("This is not a Git Working Copy; some features will not work properly.").Throttle(1000);

        builder.RuleFor(vm => vm.GitConfigFilePath)
            .WithPropertyCascadeMode(CascadeMode.Stop)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty change tracking will not be able to work as expected.")
            .Must(value => File.Exists(value), ValidationMessageType.Warning).WithMessage("This must point to a file that exists.").Throttle(1000);

        builder.RuleFor(vm => vm.GithubUserName)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty the application will be unable to push changes to GitHub.");

        builder.RuleFor(vm => vm.GithubPat)
            .NotEmpty(ValidationMessageType.Warning).WithMessage("If this is empty the application will be unable to push changes to GitHub.");

        Validator = builder.Build(this);

        // Set up an observable to check when content has actually changed.
        _stateChangeSubscription = this.WhenAnyValue(x => x.HomeLabRepoDataPath, x => x.GitConfigFilePath, x => x.GithubUserName, x => x.GithubPat,
            (repoPath, gitPath, userName, pat) =>
            {
                return new TrackedPropertyState()
                {
                    HomeLabRepoDataPath = repoPath,
                    GitConfigFilePath = gitPath,
                    GithubUserName = userName,
                    GithubPat = pat
                };
            })
            .Throttle(TimeSpan.FromSeconds(0.5))
            .Subscribe(state => HasChanges = !state.Equals(_initialState));
    }

    public override string Title => "Settings";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not SettingsNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

        HasChanges = false;

        ApplicationClassLogger.ForCaller().Information("Loading configuration settings");
        var coreConfig = _coreConfigurationManager.GetCoreConfiguration();

        HomeLabRepoDataPath = coreConfig.HomeLabRepoDataPath;
        GitConfigFilePath = coreConfig.GitConfigFilePath;
        GithubUserName = coreConfig.GithubUserName;
        GithubPat = coreConfig.GithubPat;

        // Capture the initial state of the data.
        _initialState = new TrackedPropertyState()
        {
            HomeLabRepoDataPath = HomeLabRepoDataPath,
            GitConfigFilePath = GitConfigFilePath,
            GithubUserName = GithubUserName,
            GithubPat = GithubPat
        };
    }

    public override Task<bool> TryNavigateAway()
    {
        if (!HasChanges)
        {
            ApplicationClassLogger.ForCaller().Information("Leaving page without having made any changes");
            return Task.FromResult(true);
        }
        else
        {
            ApplicationClassLogger.ForCaller().Information("Attempting to leave page with unsaved changes");
            return _sharedDialogService.ShowSimpleYesNoDialog("Unsaved changes will be lost if you continue.");
        }
    }

    public async Task SaveChangesAndNavigateBack()
    {
        IsSaving = true;

        var (dialog, dialogTask) = _sharedDialogService.ShowSimpleSavingDataDialog("Saving Core Configuration Changes...");

        ApplicationClassLogger.ForCaller().Information("Saving updated core configuration settings");

        await Task.Run(() => _coreConfigurationManager!.SaveCoreConfiguration(new CoreConfigurationDto()
        {
            HomeLabRepoDataPath = HomeLabRepoDataPath,
            GitConfigFilePath = GitConfigFilePath,
            GithubUserName = GithubUserName,
            GithubPat = GithubPat
        })).ConfigureAwait(true);

        dialog?.GetWindow().Close();

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
            _stateChangeSubscription.Dispose();
    }

    private readonly ICoreConfigurationManager _coreConfigurationManager;
    private readonly INavigationService _navigationService;
    private readonly ISharedDialogsService _sharedDialogService;
    
    private bool _hasChanges;
    private bool _isSaving;

    private string _homeLabRepoDataPath;
    private string _gitConfigFilePath;
    private string _githubUserName;
    private string _githubPat;

    private IDisposable _stateChangeSubscription;

    private TrackedPropertyState? _initialState;

    private struct TrackedPropertyState
    {
        public string HomeLabRepoDataPath;
        public string GitConfigFilePath;
        public string GithubUserName;
        public string GithubPat;
    }
}
