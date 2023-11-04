using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home;

public sealed class ServerViewModel: ReactiveObject
{
    public ServerViewModel(ServerHostDto dto)
    {
        _dto = dto ?? throw new ArgumentNullException(nameof(dto));
    }

    public string DisplayName => _dto.Metadata.DisplayName;

    public string Name => _dto.Metadata.Name;

    public string Description => _dto.Metadata.Description;

    public int DisplayIndex => _dto.Metadata.DisplayIndex;

    private readonly ServerHostDto _dto;
}
