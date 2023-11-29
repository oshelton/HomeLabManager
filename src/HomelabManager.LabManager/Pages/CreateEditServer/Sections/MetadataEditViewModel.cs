using System.Reactive.Linq;
using System.Text.RegularExpressions;
using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace HomeLabManager.Manager.Pages.CreateEditServer.Sections
{
    /// View Model for editing server metadata.
    public sealed class MetadataEditViewModel: ValidatableViewModel, IDisposable
    {
        /// Design time constructor only, should not be used otherwise.
        public MetadataEditViewModel() 
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
        public MetadataEditViewModel(BaseServerDto sourceDto, IReadOnlyList<string> allOtherDisplayNames, IReadOnlyList<string> allOtherNames) 
        {
            if (sourceDto is null)
                throw new ArgumentNullException(nameof(sourceDto));

            _allOtherDisplayNames = allOtherDisplayNames ?? throw new ArgumentNullException(nameof(allOtherDisplayNames));
            _allOtherNames = allOtherNames ?? throw new ArgumentNullException(nameof(allOtherNames));

            ServerKinds = new[]
            {
                ServerKind.Unspecified,
                ServerKind.Windows,
                ServerKind.StandardLinux,
                ServerKind.TrueNasScale,
            };

            // Set up validation.
            var builder = new ValidationBuilder<MetadataEditViewModel>();

            builder.RuleFor(vm => vm.DisplayName)
                .WithPropertyCascadeMode(CascadeMode.Stop)
                .NotEmpty(ValidationMessageType.Error).WithMessage("Server/VM display name must not be empty.")
                .Must(value => !_allOtherDisplayNames.Any(x => x.ToUpperInvariant() == value.ToUpperInvariant()), ValidationMessageType.Error).WithMessage("A server/VMs display name must be unique.").Throttle(500);

            builder.RuleFor(vm => vm.Name)
                .WithPropertyCascadeMode(CascadeMode.Stop)
                .NotEmpty(ValidationMessageType.Error).WithMessage("Server/VM host name must not be empty.")
                .Must(value => s_nameRequirementRegex.IsMatch(value)).WithMessage("Server/VM host name must complie with host name format requirements.").Throttle(500)
                .Must(value => !_allOtherNames.Any(x => x.ToUpperInvariant() == value.ToUpperInvariant()), ValidationMessageType.Error).WithMessage("A server/VMs host name must be unique.").Throttle(500);

            Validator = builder.Build(this);
            
            DisplayName = sourceDto.Metadata.DisplayName;
            Name = sourceDto.Metadata.Name;
            SelectedServerKind = ServerKinds.FirstOrDefault(x => x == sourceDto.Metadata.Kind);
            Description = sourceDto.Metadata.Description;

            // Capture the initial state of the data.
            _initialState = new TrackedPropertyState()
            {
                DisplayName = DisplayName,
                Name = Name,
                Description = Description,
                Kind = SelectedServerKind,
            };

            // Set up an observable to check when content has actually changed.
            _hasChanges = this.WhenAnyValue(x => x.DisplayName, x => x.Name, x => x.Description, x => x.SelectedServerKind)
                .Throttle(TimeSpan.FromSeconds(0.5))
                .Select(x =>
                {
                    return !(new TrackedPropertyState()
                    {
                        DisplayName = x.Item1,
                        Name = x.Item2,
                        Description = x.Item3,
                        Kind = x.Item4,
                    }.Equals(_initialState));
                })
                .ToProperty(this, nameof(HasChanges));
        }

        public void Dispose()
        {
            Validator?.Dispose();
            _hasChanges?.Dispose();
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

        /// Collection of server kinds available.
        public IReadOnlyList<ServerKind> ServerKinds { get; }

        /// Currently selected server kind.
        public ServerKind SelectedServerKind
        {
            get => _selectedServerKind;
            set => this.RaiseAndSetIfChanged(ref _selectedServerKind, value);
        }

        /// Description of the server or VM.
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        /// Whether or not the metadata has changes.
        public bool HasChanges => _hasChanges.Value;

        private static readonly Regex s_nameRequirementRegex = new Regex("^(([a-z0-9]|[a-z0-9][a-z0-9\\-]*[a-z0-9])\\.)*([a-z0-9]|[a-z0-9][a-z0-9\\-]*[a-z0-9])$", 
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly TrackedPropertyState? _initialState;
        private readonly IReadOnlyList<string> _allOtherDisplayNames;
        private readonly IReadOnlyList<string> _allOtherNames;

        private readonly ObservableAsPropertyHelper<bool> _hasChanges;

        private string _displayName;
        private string _name;
        private ServerKind _selectedServerKind;
        private string _description;

        private record struct TrackedPropertyState
        {
            public string DisplayName;
            public string Name;
            public string Description;
            public ServerKind Kind;
        }
    }
}
