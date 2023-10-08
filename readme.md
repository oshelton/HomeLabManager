# Home Lab Manager

> **Reminder:** Do not push anything containing sensetive informations, user names, public IP Addresses, or other forms of credentials to this repo.

This repo contains libraries and applications that I use to manage my soon to be further developed home lab.  These applications have the goal of making it easier to track the status of and be able to manage my home lab with software that isn't necesarily running on my actual home lab; because sometimes it's nice to know what's up when everything is on fire besides just an error page.

## License (MIT)

Copyright 2023 Jack Owen Shelton

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Contents

This repo contains two Applications:

1. **HomeLab.LabManager** - Desktop AvaloniaUI application for viewing and interacting with the various hosts in the Home Lab. The application interacts with a repo in github that contains the actual files related to the lab itself and uses Git for tracking changes.
2. **HomeLab.BackupTool** - Consolonia application responsible for backing up docker volumes and databases.
    * This may wind up not being a thing.  Not sure yet.

And a library HomeLab.Common for shared functionality between the two.

## Contributing

At this point this project is being mostly built to meet my own needs as they grow and change.  This will also be my first home lab so my plans for these applications *are* going to change regularly and offten :)

If you are interested in contributing feel free to open a PR and I'll figure out what sort of policies I need to implement regarding it at that time.

Otherwise feel free to fork the codebase and do what you want.

## Dependencies

- [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
- [AvaloniaUI Material UI](https://github.com/AvaloniaCommunity/Material.Avalonia)
- [AvaloniaUI Material UI Icons](https://github.com/AvaloniaUtils/Material.Icons.Avalonia/)
- [Avalonis XAML Behaviors](https://github.com/AvaloniaUI/Avalonia.Xaml.Behaviors)
- [Consolonia](https://github.com/jinek/Consolonia)
- [Docker.DotNet](https://github.com/dotnet/Docker.DotNet/)
- [EventBinder](https://github.com/Serg046/EventBinder)
- [libgit2sharp](https://github.com/libgit2/libgit2sharp)
- [mmcagno's SSh.NET fork](https://github.com/mmacagno/SSH.NET).
- [Moq](https://github.com/devlooped/moq)
- [NUnit](https://nunit.org/)
- [Reactive UI](https://www.reactiveui.net/) (by way of Avalonia.ReactiveUI)
- [Serilog](https://serilog.net/)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet)
