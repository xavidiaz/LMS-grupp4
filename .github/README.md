# LMS – live demo fork

This is a deployment fork of **[Lexicon-grupp4/LMS-Lexicon](https://github.com/Lexicon-grupp4/LMS-Lexicon)**, the final group project of five developers at Lexicon: a learning management system built with a Blazor Web App (as a backend-for-frontend), a separate ASP.NET Core Web API, YARP and EF Core on .NET 10.

**Live demo: <https://lms.lab.xavidiaz.com/>**
Demo login: `DemoUser@Lms.com` / `demo1234`

All development happens in the group repo; its README describes the project, the data model and how the group works. This fork adds only what is needed to run the app on a self-hosted Raspberry Pi 5 instead of Azure, and changes no application code.

## What the fork adds

| File | Purpose |
|---|---|
| [`Dockerfile`](../Dockerfile) | One Dockerfile with two targets: `api` (LMS.API) and `web` (LMS.Blazor) |
| [`.dockerignore`](../.dockerignore) | Keeps `bin/`, `obj/`, local databases and the like out of the build |
| [`.github/workflows/publish.yml`](workflows/publish.yml) | Builds both images for arm64 and pushes them to ghcr.io |
| `.github/README.md` | This file (shown instead of the group's README, which stays untouched) |

The group's Azure workflow (`main_lms-grupp4.yml`) is disabled in this fork.

## How it is deployed

```
group repo (main) ──Sync fork──▶ this fork (main)
                                   │  GitHub Actions, ubuntu-24.04-arm
                                   ▼
             ghcr.io/xavidiaz/lms-api   ghcr.io/xavidiaz/lms-web
                                   │  podman auto-update, every 5 min
                                   ▼
            Raspberry Pi 5 (NixOS): nginx (TLS) ──▶ web ──▶ api
```

1. Changes merged to `main` in the group repo reach this fork with GitHub's **Sync fork** button.
2. Every push to `main` runs `publish.yml`, which builds the `api` and `web` images natively on an arm64 runner and tags them `latest` plus the commit SHA.
3. The Pi checks for new images every five minutes and restarts the containers. If a new image fails to start, it rolls back to the previous one.

The Pi's side is declared in its NixOS configuration (not public). In short:

- **web** is the only public part, behind nginx with TLS at `lms.lab.xavidiaz.com`.
- **api** has no public port. Only the web container reaches it, at a fixed address on the internal container network. The web container finds it through `ReverseProxy__Clusters__remote-api__Destinations__primary__Address`.
- Both containers run hardened: read-only root filesystem, a small `/tmp` tmpfs, all capabilities dropped, and a non-root user. Login form posts are rate-limited.

## Why SQLite instead of SQL Server

The Pi is an arm64 machine. Microsoft's SQL Server images exist only for amd64, and Azure SQL Edge, the retired arm64 variant, doesn't start on the Pi 5's kernel (16K memory pages). So the `api` image uses SQLite. The Dockerfile makes these changes during the build only:

- it adds `Microsoft.EntityFrameworkCore.Sqlite`, matching the project's EF Core version
- it replaces `UseSqlServer(...)` with `UseSqlite(...)` in `LMS.API/Program.cs`
- it creates the schema from the model with `EnsureCreated()` on startup, because the SQL Server migrations don't apply to SQLite

If those lines change in the group repo, the build fails with an error, so a broken image can't be deployed.

The database lives in `/tmp`, so **the demo data resets every time the API restarts or updates**. The API runs with `ASPNETCORE_ENVIRONMENT=Development`, because the app seeds its demo user only there. That is safe here because the API can't be reached from outside.

## Configuration on the Pi

| Container | Setting | Value |
|---|---|---|
| api | `ConnectionStrings__ApplicationDbContext` | `Data Source=/tmp/lms.db` |
| api | `JwtSettings__SecretKey` | generated on the Pi, kept out of git |
| api | `password` | demo user's password (`demo1234`) |
| web | `ReverseProxy__…__Address` | the API's internal address |
| web | `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true`: TLS ends at nginx, so redirects need `X-Forwarded-Proto` |

The web app saves login sessions to `App_Data/tokens.json`. Since the root filesystem is read-only, the `web` image points `App_Data` at `/tmp`.

## Running the images locally

```bash
docker build --target api -t lms-api .
docker build --target web -t lms-web .
docker network create lms
docker run -d --name api --network lms \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__ApplicationDbContext=Data Source=/tmp/lms.db" \
  -e password=demo1234 \
  -e JwtSettings__SecretKey=$(openssl rand -hex 32) lms-api
docker run -d --name web --network lms -p 8080:8080 \
  -e "ReverseProxy__Clusters__remote-api__Destinations__primary__Address=http://api:8080/" lms-web
```

The login cookie is `Secure`, so log in through `http://localhost:8080`: browsers treat `localhost` as a secure origin.
