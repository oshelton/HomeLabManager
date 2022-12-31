using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages
{
    /// <summary>
    /// Base class for primary navigation pages.
    /// </summary>
    public abstract class PageBaseViewModel: ReactiveObject
    {
        /// Title of this page.
        public abstract string Title { get; }

        /// <summary>
        /// Activate the page.
        /// </summary>
        public abstract void Activate();

        /// <summary>
        /// Attempt to deactivate the page.
        /// </summary>
        /// <returns>True if the page deactivated and can be navigated away from, false otherwise.</returns>
        public abstract bool TryDeactivate();
    }
}
