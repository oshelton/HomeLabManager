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
        Program.ServiceProvider!.Services.GetService<ICoreConfigurationManager>()!.GetOrCreateCoreConfiguration(() => new CoreConfigurationDto
        {
            GitConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gitconfig"),
            GithubPat = "",
            GithubUserName = "",
            HomeLabRepoDataPath = "",
        });

        _navigationService = Program.ServiceProvider!.Services.GetService<INavigationService>()!;
    }

    /// <summary>
    /// Once the window loads navigate to the Home Page.
    /// </summary>
    public async void WindowLoaded()
    {
        await NavigationService.NavigateTo(new HomeNavigationRequest()).ConfigureAwait(false);
    }

    /// <summary>
    /// Reference to the Navigation Service.
    /// </summary>
    public INavigationService NavigationService => _navigationService;

    private readonly INavigationService _navigationService;
}
