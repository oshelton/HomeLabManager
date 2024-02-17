# Home Lab Manager

> **Archived:** This project is no longer seeing active development. For the purposes of managing my homelab a combination of [Obsidian](https://obsidian.md/), [Obsidian DataView](https://github.com/blacksmithgu/obsidian-dataview), [Obsidian Metadata Menu](https://mdelobelle.github.io/metadatamenu/), and [XPipe](https://xpipe.io/) get me close enough to something that meets all of my needs without all of the ongoing maintenance and effort that creating my own application entails, and delays in getting things off the ground while I get things implemented.

> **Reminder:** Do not push anything containing sensetive informations, user names, public IP Addresses, or other forms of credentials to this repo.

This repo contains libraries and applications that I use to manage my soon to be further developed home lab.  These applications have the goal of making it easier to track the status of and be able to manage my home lab with software that isn't necesarily running on my actual home lab; because sometimes it's nice to know what's up when everything is on fire besides just an error page.

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

## Building

For the UI application simply open the HomeLabManager solution file and run the HomeLabManager.Manager project.

## Running Tests

In order to be able to run the tests successfully you will need to create a couple of things:

1. Create the following files in the tests\TestData directory.
    * github_pat.txt - Containing a valid Github PAT that can be used to commit and push code.
        * I strongly recomend creating a new PAT with as minimal permissions as possible (specifically limited to the repo in the url in the github_repo.txt file) and not reuse an existing one.
    * github_username.txt - Containing a valid github user name/email associated with the above PAT.
    * github_repo.txt - Containing a valid url to a Github repo where test data will be pushed to and later deleted from.

You can confirm by examining the code that nothing untoward is done with the above information.  An important part of how the manager works is that it uses Github to store homelab information and the tests exercise that capavility.

## Dependencies

- [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
- [Avalonia UI Edit](https://github.com/AvaloniaUI/AvaloniaEdit)
- [AvaloniaUI Material UI](https://github.com/AvaloniaCommunity/Material.Avalonia)
- [AvaloniaUI Material UI Icons](https://github.com/AvaloniaUtils/Material.Icons.Avalonia/)
- [Avalonis XAML Behaviors](https://github.com/AvaloniaUI/Avalonia.Xaml.Behaviors)
- [Consolonia](https://github.com/jinek/Consolonia)
- [Docker.DotNet](https://github.com/dotnet/Docker.DotNet/)
- [EventBinder](https://github.com/Serg046/EventBinder)
- [libgit2sharp](https://github.com/libgit2/libgit2sharp)
- [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia)
- [mmcagno's SSh.NET fork](https://github.com/mmacagno/SSH.NET).
- [Moq](https://github.com/devlooped/moq)
- [NUnit](https://nunit.org/)
- [Reactive UI](https://www.reactiveui.net/) (by way of Avalonia.ReactiveUI)
- [Serilog](https://serilog.net/)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet)
