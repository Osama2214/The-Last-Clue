# Operational checks

| Probe | Foundation response | Intended completed behavior |
| --- | --- | --- |
| GET /health | 200, status Healthy | Process liveness |
| GET /health/ready | 503, status Unhealthy | SQL reachable, core configuration valid and five complete reviewed case families available |

Both responses expose the status only and are not cached. The configuration readiness check detects a missing or malformed ConnectionStrings:Default value and reports the key name internally, without exposing its value. It does not open a database connection. AI-disabled, missing-key and known-outage settings do not by themselves fail configuration readiness. The readiness check deliberately stays unhealthy until the actual persistence and case coverage checks replace the foundation check. A successful build or liveness response does not demonstrate a playable game.

The first deployment will use one Azure App Service instance and Azure SQL. No hosting resources or provider credentials are created by the repository foundation. CI publishes a downloadable artifact; application deployment and post-release smoke checks are separate backlog tasks.

## Database release

The first release script is [db/release-001.sql](../db/release-001.sql). It creates the Identity and game tables, foreign keys, indexes and constraints. EF records the migration in the target database history so a second run skips it.

1. Confirm the target server and database with the release owner. Create the empty database for the first release. For an existing database, take a backup or confirm a usable restore point before applying the script.
2. Run the reviewed script as a release step using a SQL client that supports GO batches and stops on errors. With sqlcmd, add the environment's authentication options to this command and keep credentials outside the script.

   ```powershell
   sqlcmd -S "<server>" -d "<database>" -b -i db/release-001.sql
   ```
3. Verify that dbo.__EFMigrationsHistory contains InitialCreate, then deploy the application. The web application does not apply migrations at startup.

To regenerate the first release script from the repository root, save its opening SET statements and GO batch first. Restore that batch at the top of the generated file so filtered indexes can be created with any supported client:

```powershell
dotnet tool restore
dotnet build src/LastClue.Web --configuration Release
dotnet ef migrations script 0 InitialCreate --idempotent --project src/LastClue.Infrastructure --startup-project src/LastClue.Web --configuration Release --no-build --output db/release-001.sql
```
