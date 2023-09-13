using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home;

public sealed class ServerViewModel: ReactiveObject
{
    public ServerViewModel(ServerHostDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        Name = dto.Metadata.Name;
        DisplayIndex = dto.Metadata.DisplayIndex;
    }

    public string Name { get; }

    public int DisplayIndex { get; }
}
