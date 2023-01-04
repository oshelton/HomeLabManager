using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Common.Data.Git;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.HomeComponents;

public sealed class ServerViewModel: ReactiveObject
{
    public ServerViewModel(ServerDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        Name = dto.Metadata?.Name;
    }

    public string? Name { get; }
}
