# HomeLab

> **Reminder to Self:** Do not push anything containing sensetive keys, user names, public IP Addresses, or other forms of credentials to this repo.

This repo contains libraries and applications that I use to manage my soon to be further developed home lab.

## Contents

This repo contains two Applications:

1. **HomeLab.LabManager** - Desktop AvaloniaUI application for viewing and interacting with the various hosts in the Home Lab. The application interacts with a repo in github that contains the actual files related to the lab itself and uses Git for tracking changes.
2. **HomeLab.BackupTool** - Consolonia application responsible for backing up docker volumes and databases.

And a library HomeLab.Common for shared functionality.

## Dependencies

-   [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
-   [AvaloniaUI Material UI](https://github.com/AvaloniaCommunity/Material.Avalonia)
-   [AvaloniaUI Material UI Icons](https://github.com/AvaloniaUtils/Material.Icons.Avalonia/)
-   [Consolonia](https://github.com/jinek/Consolonia)
-   [Docker.DotNet](https://github.com/dotnet/Docker.DotNet/)
-   [libgit2sharp](https://github.com/libgit2/libgit2sharp)
-   [mmcagno's SSh.NET fork](https://github.com/mmacagno/SSH.NET).
-   [YamlDotNet](https://github.com/aaubry/YamlDotNet)
