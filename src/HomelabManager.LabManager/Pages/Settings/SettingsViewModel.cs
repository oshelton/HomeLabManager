using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.Manager.Utils;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Settings
{
    /// <summary>
    /// Home Page View Model.
    /// </summary>
    public sealed class SettingsViewModel : PageBaseViewModel
    {
        public SettingsViewModel() { }

        public override string Title => "Settings";

        public override async Task NavigateTo(INavigationRequest request)
        {
            if (request is not SettingsNavigationRequest)
                throw new InvalidOperationException("Expected navigation request type is HomeNavigationRequest.");
        }

        public override Task<bool> TryNavigateAway() => Task.FromResult(true);
    }
}
