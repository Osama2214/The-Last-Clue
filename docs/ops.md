# Operational checks

| Probe | Foundation response | Intended completed behavior |
| --- | --- | --- |
| GET /health | 200, status Healthy | Process liveness |
| GET /health/ready | 503, status Unhealthy | SQL reachable, core configuration valid and five complete reviewed case families available |

Both responses expose the status only and are not cached. The readiness check deliberately stays unhealthy until the actual persistence and case coverage checks replace the foundation check. A successful build or liveness response does not demonstrate a playable game.

The first deployment will use one Azure App Service instance and Azure SQL. No hosting resources or provider credentials are created by the repository foundation. CI publishes a downloadable artifact; deployment, migrations and release smoke checks are separate backlog tasks.
