using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home;

/// <summary>
/// View Model for servers on the home page.
/// </summary>
public sealed class ServerViewModel: ReactiveObject
{
    public ServerViewModel(ServerHostDto dto)
    {
        _dto = dto ?? throw new ArgumentNullException(nameof(dto));
    }

    public ServerKind Kind => _dto.Metadata.Kind ?? ServerKind.Unspecified;

    public string DisplayName => _dto.Metadata.DisplayName;

    public string Name => _dto.Metadata.Name;

    public string Description => _dto.Metadata.Description;

    public int DisplayIndex => _dto.Metadata.DisplayIndex;

    private readonly ServerHostDto _dto;
}
