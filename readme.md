# HomeLab

> **Reminder to Self:** Do not push anything containing sensetive keys, user names, public IP Addresses, or other forms of credentials to this repo.

This repo contains libraries and application that I use to manage my soon to be further developed home lab.

## Contents

This repo contains the following:

-   Managing my home lab environment; including.
    -   Simple provisioning of Linux servers.
    -   Managing of Linux based Docker Engines.
    -   Managing Docker Desktop instances on Windows.
    -   Manual backing up and restoring of docker containers and volumes.
-   Automatic backing up of containers on the Linux server they are running on for managed docker containers.
-   Utility library for shared configuration management, change tracking, SSH interactions, and Docker management.

## Dependencies

-   [mmcagno's SSh.NET fork](https://github.com/mmacagno/SSH.NET).
-   [Docker.DotNet](https://github.com/dotnet/Docker.DotNet/)
