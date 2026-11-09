# Operational checks

| Probe | Behavior |
| --- | --- |
| GET /health | Returns 200 while the web process is serving requests |
| GET /health/ready | Returns 200 when core configuration is valid and SQL Server is reachable, or 503 otherwise |

Both probes return only the status and disable caching. Configure `ConnectionStrings:Default` for the target SQL Server. Missing AI credentials, a disabled provider or a known outage still permit database readiness. Reviewed case coverage will be added to the readiness probe alongside the local case library.

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

## Bootstrap the administrator

Apply the database migration before running the bootstrap command. Configure `Admin:Email` and `Admin:Password` using local user secrets or the hosting environment. Use `Admin__Email` and `Admin__Password` as environment variable names. Run the command once against the target database.

```powershell
dotnet run --project src/LastClue.Web --no-launch-profile -- --seed-identity
```

The command creates the Player and Admin roles and the first administrator account. Repeating it for the same administrator leaves the account in place. Remove the bootstrap secrets afterward. Application cookies use an eight-hour sliding window. The Compose deployment persists data-protection keys in a named volume. Configure a persistent, restricted key store when deploying to Azure.
