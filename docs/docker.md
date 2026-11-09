# Run with Docker

Docker Compose runs the current web application in a Linux container. Docker Desktop must be running with Linux containers enabled. The host does not need a .NET SDK for this workflow.

Run these commands from `Source Code`:

```sh
docker compose up --build --detach --wait
docker compose ps
```

Open **http://localhost:8080**. The port is published on the local machine only. The first build downloads the official .NET images and can take several minutes.

## Everyday commands

```sh
# Follow application logs. Ctrl+C stops following logs.
docker compose logs --follow web

# Rebuild and apply source changes.
docker compose up --build --detach --wait

# Stop and remove this project's containers and network, retaining the key volume.
docker compose down
```

There is no source mount or automatic reload. Rebuild after code changes. Do not use `docker compose down --volumes` for a normal stop: it also deletes the saved data-protection keys.

If port 8080 is already in use, choose another local port in PowerShell before starting:

```powershell
$env:LAST_CLUE_HTTP_PORT = "8081"
docker compose up --build --detach --wait
```

Then open `http://localhost:8081`.

## Container layout

The Dockerfile restores locked dependencies and publishes the web project with the pinned SDK. The final image contains the ASP.NET runtime and published output, and runs as the image's non-root application user. Build outputs, documentation, tests and local secret files are excluded from the build context.

Compose stores ASP.NET data-protection keys in a named volume so container replacement does not discard them. This is independent of game data. The current application has no database integration; the SQL Server service and connection configuration will be added with the persistence implementation.

The Compose environment is Development and uses local HTTP. For hosting, configure HTTPS and the production environment through the hosting platform. This local setup does not replace the deployment plan.

## Health checks

| Endpoint | Current response | Meaning |
| --- | --- | --- |
| `/health` | 200 | The web process is alive; Docker uses this endpoint for its health check |
| `/health/ready` | 503 | Game storage, configuration and reviewed case coverage are not implemented yet |

A healthy container confirms that the web host runs, not that gameplay is complete. Startup, home-page content, static CSS, liveness, readiness and the runtime user are checked in CI.

## Image updates

The base image digests are pinned in the Dockerfile. When updating the SDK, keep its version aligned with `global.json`, update the corresponding image digest and run the container checks again. Update the ASP.NET runtime image digest when adopting a new runtime patch.

References: [Microsoft container guidance](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/building-net-docker-images?view=aspnetcore-10.0), [Docker Compose commands](https://docs.docker.com/compose/intro/compose-application-model/).
