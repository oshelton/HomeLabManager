﻿using System.Diagnostics;
using EventBinder;
using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager;

/// <summary>
/// View Model for the Main Window.
/// </summary>
public sealed class MainWindowViewModel: ReactiveObject
{
    public MainWindowViewModel()
    {
        var coreConfigurationManager = Program.ServiceProvider!.Services.GetService<ICoreConfigurationManager>()!;
        coreConfigurationManager.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
        {
            GitConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitconfig"),
            GithubPat = "",
            GithubUserName = "",
            HomeLabRepoDataPath = "",
        });
        coreConfigurationManager.CoreConfigurationUpdated.Subscribe(UpdateConfigProperties);

        UpdateConfigProperties(coreConfigurationManager.GetCoreConfiguration());

        _navigationService = Program.ServiceProvider!.Services.GetService<INavigationService>()!;
    }

    /// <summary>
    /// Whether or not the repo data path point to a valid directory.
    /// </summary>
    public bool HasRepoDataPath
    {
        get => _hasRepoDataPath;
        set => this.RaiseAndSetIfChanged(ref _hasRepoDataPath, value);
    }

    /// <summary>
    /// Path to the data repo.
    /// </summary>
    public string? RepoDataPath
    {
        get => _repoDataPath;
        set => this.RaiseAndSetIfChanged(ref _repoDataPath, value);
    }

    /// <summary>
    /// Reference to the Navigation Service.
    /// </summary>
    public INavigationService NavigationService => _navigationService;

    /// <summary>
    /// Once the window loads navigate to the Home Page.
    /// </summary>
    public async Task WindowLoaded()
    {
        await NavigationService.NavigateTo(new HomeNavigationRequest()).ConfigureAwait(false);
    }

    public void OpenRepoDataPath()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            Arguments = RepoDataPath,
            FileName = "explorer.exe"
        };
        Process.Start(startInfo);
    }

    private void UpdateConfigProperties(CoreConfigurationDto config)
    {
        HasRepoDataPath = Directory.Exists(config.HomeLabRepoDataPath);
        RepoDataPath = config.HomeLabRepoDataPath;
    }

    private readonly INavigationService _navigationService;

    private bool _hasRepoDataPath;
    private string? _repoDataPath;
}
