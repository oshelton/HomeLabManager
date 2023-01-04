using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Manager.Pages;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager
{
    public sealed class MainWindowViewModel : ReactiveObject
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

            _currentPage = Pages[0];
            _currentPage.Activate();
        }

        public bool CanNavigateBack
        {
            get => _canNavigateBack;
            set => this.RaiseAndSetIfChanged(ref _canNavigateBack, value);
        }

        public PageBaseViewModel? CurrentPage
        {
            get => _currentPage;
            set
            {
                if (value != _currentPage)
                {
                    _currentPage?.TryDeactivate();
                    this.RaiseAndSetIfChanged(ref _currentPage, value);
                    _currentPage?.Activate();
                }
            }
        }

        public IReadOnlyList<PageBaseViewModel> Pages { get; } = new[]
        {
            new HomeViewModel()
        };

        private PageBaseViewModel? _currentPage;
        private bool _canNavigateBack;
    }
}
