﻿using System.Diagnostics;
using System.Reactive.Linq;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.SharedDialogs;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Settings;

/// <summary>
/// Settings Page View Model.
/// </summary>
public sealed class SettingsViewModel : PageBaseViewModel
{
    public override string Title => "Settings";

    public override async Task NavigateTo(INavigationRequest request)
    {
        if (request is not SettingsNavigationRequest)
            throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");

        _coreConfigurationManager = Program.ServiceProvider!.Services.GetService<ICoreConfigurationManager>()!;
        _navigationService = Program.ServiceProvider!.Services.GetService<INavigationService>()!;

        HasChanges = false;

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

    public override async Task<bool> TryNavigateAway()
    {
        if (!HasChanges)
        {
            _stateChangeSubscription!.Dispose();
            return true;
        }

        var dialogModel = new ConfirmLeaveDialogViewModel();
        await DialogHost.Show(dialogModel, MainWindow.MainDialogHostId).ConfigureAwait(true);

        return dialogModel.DoLeave;
    }

    public async Task OpenFolderPickerForRepoPath()
    {
        var chosenFolder = await MainWindow.Instance!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Pick a .gitconfig File"
        }).ConfigureAwait(true);

        if (chosenFolder is not null && chosenFolder.Count == 1)
        {
            chosenFolder[0].TryGetUri(out var uri);
            HomeLabRepoDataPath = uri!.LocalPath;
        }
    }

    public async Task OpenFilePickerForGitConfig()
    {
        var openedFile = await MainWindow.Instance!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "Pick a .gitconfig File"
        }).ConfigureAwait(true);

        if (openedFile is not null && openedFile.Count == 1)
        {
            openedFile[0].TryGetUri(out var uri);
            GitConfigFilePath = uri!.LocalPath;
        }
    }

    public async Task SaveChangesAndNavigateBack()
    {
        IsSaving = true;

        var dialogTask = DialogHost.Show(new SimpleSavingDataDialogViewModel() { SaveMessage = "Saving Core Configuration Changes..." }, MainWindow.MainDialogHostId);

        await Task.Run(() => _coreConfigurationManager!.SaveCoreConfiguration(new CoreConfigurationDto()
        {
            HomeLabRepoDataPath = HomeLabRepoDataPath,
            GitConfigFilePath = GitConfigFilePath,
            GithubUserName = GithubUserName,
            GithubPat = GithubPat
        })).ConfigureAwait(true);

        DialogHost.Close(MainWindow.MainDialogHostId);

        IsSaving = false;
        HasChanges = false;

        await _navigationService!.NavigateBack().ConfigureAwait(false);
    }

    public INavigationService? NavigationService => _navigationService;

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

    public string? HomeLabRepoDataPath
    {
        get => _homeLabRepoDataPath;
        set => this.RaiseAndSetIfChanged(ref _homeLabRepoDataPath, value);
    }

    public string? GitConfigFilePath
    {
        get => _gitConfigFilePath;
        set => this.RaiseAndSetIfChanged(ref _gitConfigFilePath, value);
    }

    public string? GithubUserName
    {
        get => _githubUserName;
        set => this.RaiseAndSetIfChanged(ref _githubUserName, value);
    }

    public string? GithubPat
    {
        get => _githubPat;
        set => this.RaiseAndSetIfChanged(ref _githubPat, value);
    }

    private ICoreConfigurationManager? _coreConfigurationManager;
    private INavigationService? _navigationService;
    
    private bool _hasChanges;
    private bool _isSaving;

    private string? _homeLabRepoDataPath;
    private string? _gitConfigFilePath;
    private string? _githubUserName;
    private string? _githubPat;

    private IDisposable? _stateChangeSubscription;

    private TrackedPropertyState? _initialState;

    private struct TrackedPropertyState
    {
        public string? HomeLabRepoDataPath;
        public string? GitConfigFilePath;
        public string? GithubUserName;
        public string? GithubPat;
    }
}
