using System.Collections.Specialized;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.ServerListing;

public sealed class ServerViewModel : ReactiveObject, IDisposable
{
    /// <summary>
    /// Main constructor.
    /// </summary>
    /// <param name="dto">Dto backing the view model.</param>
    public ServerViewModel(ServerHostDto dto, ServerListingViewModel listingViewModel, int startingServerCount)
    {
        _dto = dto ?? throw new ArgumentNullException(nameof(dto));
        ParentListingViewModel = listingViewModel ?? throw new ArgumentNullException(nameof(listingViewModel));

        _displayIndex = _dto.Metadata.DisplayIndex;

        _disposables = new CompositeDisposable();

        // Set up an observable to tracke whether or not this server can be moved up.
        _canMoveUp = this.WhenAnyValue(x => x.DisplayIndex, displayIndex => displayIndex > 0)
            .ToProperty(this, nameof(CanMoveUp))
            .DisposeWith(_disposables);

        // Set up an observable to tracke whether or not this server can be moved down.
        _canMoveDown = Observable.CombineLatest(this.WhenAnyValue(x => x.DisplayIndex), 
                        Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                            h => ((INotifyCollectionChanged) ParentListingViewModel.SortedServers).CollectionChanged += h,
                            h => ((INotifyCollectionChanged) ParentListingViewModel.SortedServers).CollectionChanged -= h)
                            .Select(handlerArgs => ParentListingViewModel.SortedServers.Count), 
                        (index, serverCount) => index < serverCount - 1)
            .StartWith(_displayIndex < startingServerCount - 1)
            .ToProperty(this, nameof(CanMoveDown))
            .DisposeWith(_disposables);

    }

    /// <summary>
    /// Design time default constructor.
    /// </summary>
    public ServerViewModel()
        : this(new ServerHostDto
        {
            Metadata = new ServerMetadataDto
            {
                DisplayName = "Test Display Name",
                Name = "TEST-HOST-NAME",
                Description = @"# Description
Good morning **America**!

* List item 1
* List item 2

> Quote

```csharp
var x = 0;
Debug.WriteLine(x);
```",
                DisplayIndex = 5,
            }
        }, new ServerListingViewModel(), 0) { }

    /// <summary>
    /// Cleanup the view model.
    /// </summary>
    public void Dispose() => _disposables.Dispose();

    /// <summary>
    /// Unique Id of the Server.
    /// </summary>
    public Guid UniqueId => _dto.UniqueId ?? Guid.Empty;

    /// <summary>
    /// Display name of the Server.
    /// </summary>
    public string DisplayName => _dto.Metadata.DisplayName;

    /// <summary>
    /// Name/Host Name of the server.
    /// </summary>
    public string Name => _dto.Metadata.Name;

    /// <summary>
    /// Description of the server.
    /// </summary>
    public string Description => _dto.Metadata.Description;

    /// <summary>
    /// Display index/order of the server.
    /// </summary>
    public int DisplayIndex
    {
        get => _displayIndex;
        set => this.RaiseAndSetIfChanged(ref _displayIndex, value);
    }

    /// <summary>
    /// Whether or not this server can be moved up.
    /// </summary>
    public bool CanMoveUp => _canMoveUp.Value;

    /// <summary>
    /// Whether or not this server can be moved down.
    /// </summary>
    public bool CanMoveDown => _canMoveDown.Value;

    /// <summary>
    /// Reference back to the parent listing view model.
    /// </summary>
    public ServerListingViewModel ParentListingViewModel { get; }

    /// <summary>
    /// Reconstruct a dto from this Server view model.
    /// </summary>
    public ServerHostDto ToDto() => _dto with
    {
        Metadata = _dto.Metadata with { DisplayIndex = DisplayIndex },
    };

    private readonly ServerHostDto _dto;
    private readonly CompositeDisposable _disposables;
    private readonly ObservableAsPropertyHelper<bool> _canMoveUp;
    private readonly ObservableAsPropertyHelper<bool> _canMoveDown;

    private int _displayIndex;
}
