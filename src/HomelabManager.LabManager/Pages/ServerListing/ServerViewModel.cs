using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Collections;
using HomeLabManager.Common.Data.Git.Server;
using ReactiveUI;

namespace HomeLabManager.Manager.Pages.ServerListing;

public sealed class ServerViewModel : ReactiveObject
{
    public ServerViewModel(ServerHostDto dto)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        DisplayName = dto.Metadata.DisplayName;
        Name = dto.Metadata.Name;
        DisplayIndex = dto.Metadata.DisplayIndex;
    }

    public string DisplayName { get; }
    public string Name { get; }
    public int DisplayIndex { get; }
}
