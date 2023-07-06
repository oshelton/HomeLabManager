using System.Reactive.Linq;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.Settings;
using ReactiveUI;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace HomeLabManager.Manager.Pages.CreateEditServer.Sections
{
    /// View Model for editing server metadata.
    public sealed class MetadataViewModel: ValidatableViewModel, IDisposable
    {
        /// Design time constructor only, should not be used otherwise.
        public MetadataViewModel() 
            : this(new ServerHostDto
            {
                Metadata = new ServerMetadataDto { DisplayName = "New Server", Name = "TEST-NEW-SERVER" },
                Configuration = new ServerConfigurationDto(),
                DockerCompose = new DockerComposeDto(),
                VMs = Array.Empty<ServerVmDto>(),
            }, Array.Empty<string>(), Array.Empty<string>())
        {}

        /// <summary>
        /// Constructor for consumers to use.
        /// </summary>
        /// <param name="sourceDto">Base server dto to be edited.</param>
        /// <param name="allOtherDisplayNames">All other Server/VM display names, used for validation testing.</param>
        /// <param name="allOtherNames">All other Server/VM host names, used for validation purposes.</param>
        public MetadataViewModel(BaseServerDto sourceDto, IReadOnlyList<string> allOtherDisplayNames, IReadOnlyList<string> allOtherNames) 
        {
            if (sourceDto is null)
                throw new ArgumentNullException(nameof(sourceDto));

            _allOtherDisplayNames = allOtherDisplayNames ?? throw new ArgumentNullException(nameof(allOtherDisplayNames));
            _allOtherNames = allOtherNames ?? throw new ArgumentNullException(nameof(allOtherNames));

            // Set up validation.
            var builder = new ValidationBuilder<MetadataViewModel>();

            builder.RuleFor(vm => vm.DisplayName)
                .WithPropertyCascadeMode(CascadeMode.Stop)
                .NotEmpty(ValidationMessageType.Error).WithMessage("Server/VM display name must not be empty.")
                .Must(value => _allOtherDisplayNames.Any(x => x.ToUpperInvariant() == value.ToUpperInvariant()), ValidationMessageType.Error).WithMessage("A server/VMs display name must be unique.").Throttle(500);

            builder.RuleFor(vm => vm.Name)
                .WithPropertyCascadeMode(CascadeMode.Stop)
                .NotEmpty(ValidationMessageType.Error).WithMessage("Server/VM host name must not be empty.")
                .Must(value => _allOtherNames.Any(x => x.ToUpperInvariant() == value.ToUpperInvariant()), ValidationMessageType.Error).WithMessage("A server/VMs host name must be unique.").Throttle(500);

            Validator = builder.Build(this);

            DisplayName = sourceDto.Metadata.DisplayName;
            Name = sourceDto.Metadata.Name;

            // Capture the initial state of the data.
            _initialState = new TrackedPropertyState()
            {
                DisplayName = DisplayName,
                Name = Name,
            };

            // Set up an observable to check when content has actually changed.
            _hasChangesSubscription = this.WhenAnyValue(x => x.DisplayName, x => x.Name,
                (displayName, name) =>
                {
                    return new TrackedPropertyState()
                    {
                        DisplayName = displayName,
                        Name = name
                    };
                })
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Subscribe(state => HasChanges = !state.Equals(_initialState));
        }

        public void Dispose()
        {
            Validator?.Dispose();
            _hasChangesSubscription?.Dispose();
        }

        /// The display name of the server or VM.
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        /// Hostname/computer name of the server or VM.
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        /// Whether or not the metadata has changes.
        public bool HasChanges
        {
            get => _hasChanges;
            private set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
        }

        private string _displayName;
        private string _name;
        private bool _hasChanges;

        private readonly IDisposable _hasChangesSubscription;

        private TrackedPropertyState? _initialState;
        private IReadOnlyList<string> _allOtherDisplayNames;
        private IReadOnlyList<string> _allOtherNames;


        private struct TrackedPropertyState
        {
            public string DisplayName;
            public string Name;
        }
    }
}
