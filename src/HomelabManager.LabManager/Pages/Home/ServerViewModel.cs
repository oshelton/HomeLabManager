using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.Home;

public sealed class ServerViewModel: ReactiveObject
{
    public ServerViewModel(ServerHostDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        Name = dto.Metadata?.Name;
    }

    public string Name { get; }
}
