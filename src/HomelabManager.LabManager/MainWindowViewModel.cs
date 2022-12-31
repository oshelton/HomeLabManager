using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Manager.Pages;
using ReactiveUI;

namespace HomeLabManager.Manager
{
    public sealed class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
        {
            m_currentPage = Pages[0];
        }

        public bool CanNavigateBack
        {
            get => m_canNavigateBack;
            set => this.RaiseAndSetIfChanged(ref m_canNavigateBack, value);
        }

        public PageBaseViewModel CurrentPage
        {
            get => m_currentPage;
            set => this.RaiseAndSetIfChanged(ref m_currentPage, value);
        }

        public IReadOnlyList<PageBaseViewModel> Pages { get; } = new[]
        {
            new HomeViewModel()
        };

        private PageBaseViewModel m_currentPage;
        private bool m_canNavigateBack;
    }
}
