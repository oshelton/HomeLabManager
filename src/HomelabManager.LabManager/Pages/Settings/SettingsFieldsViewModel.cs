using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using HomeLabManager.Common.Data.CoreConfiguration;
using LibGit2Sharp;
using ReactiveUI;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace HomeLabManager.Manager.Pages.Settings
{
    /// <summary>
    /// View Model representing the various configuration settings.
    /// </summary>
    public class SettingsFieldsViewModel : ValidatableViewModel
    {
        public SettingsFieldsViewModel(CoreConfigurationDto config) 
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            var builder = new ValidationBuilder<SettingsFieldsViewModel>();

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

            // Set up observable to monitor for state changes.
            _hasChanges = this.WhenAnyValue(x => x.Name, x => x.IsActive, x => x.HomeLabRepoDataPath, x => x.GitConfigFilePath, x => x.GithubUserName, x => x.GithubPat,
                (name, isActive, repoPath, gitPath, userName, pat) =>
                {
                    return new TrackedPropertyState()
                    {
                        Name = name,
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
                .ToProperty(this, nameof(HasChanges));

            // Set up an observable to check when content has actually changed and there are no errors.
            _canSave = this.WhenAnyValue(x => x.HasErrors, x => x.HasChanges,
                (hasErrors, hasChanges) => !hasErrors && hasChanges)
                .ToProperty(this, nameof(CanSave));

            Name = config.Name;
            IsActive = config.IsActive;
            HomeLabRepoDataPath = config.HomeLabRepoDataPath;
            GitConfigFilePath = config.GitConfigFilePath;
            GithubUserName = config.GithubUserName;
            GithubPat = config.GithubPat;

            // Capture the initial state of the data.
            _initialState = new TrackedPropertyState()
            {
                Name = Name,
                IsActive = IsActive,
                HomeLabRepoDataPath = HomeLabRepoDataPath,
                GitConfigFilePath = GitConfigFilePath,
                GithubUserName = GithubUserName,
                GithubPat = GithubPat
            };
        }

        /// Whether or not this page has changes and is in a valid state to be saved.
        public bool CanSave => _canSave.Value;

        /// Whether or not this page has changes, regardless of whether or not they are valid.
        public bool HasChanges => _hasChanges.Value;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
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

        public CoreConfigurationDto MergeInChanges(CoreConfigurationDto config)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            return config with
            {
                Name = Name,
                IsActive = IsActive,
                HomeLabRepoDataPath = HomeLabRepoDataPath,
                GitConfigFilePath = GitConfigFilePath,
                GithubUserName = GithubUserName,
                GithubPat = GithubPat
            };
        }

        private readonly ObservableAsPropertyHelper<bool> _canSave;
        private readonly ObservableAsPropertyHelper<bool> _hasChanges;

        private string _name;
        private bool _isActive;
        private string _homeLabRepoDataPath;
        private string _gitConfigFilePath;
        private string _githubUserName;
        private string _githubPat;

        private TrackedPropertyState? _initialState;

        private struct TrackedPropertyState
        {
            public string Name;
            public bool IsActive;
            public string HomeLabRepoDataPath;
            public string GitConfigFilePath;
            public string GithubUserName;
            public string GithubPat;
        }
    }
}
