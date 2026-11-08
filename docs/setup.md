# Local setup

## Requirements

- Git.
- .NET SDK 10.0.401, pinned in `global.json`; later patches in that feature band are permitted.
- Any editor with C# support.

The foundation runs without SQL Server or an AI provider. Add those prerequisites when the persistence and provider tasks are implemented; do not treat the scaffold as a finished game.

## Build and run

```sh
git clone https://github.com/Osama2214/The-Last-Clue.git
cd The-Last-Clue
dotnet restore LastClue.sln --locked-mode
dotnet build LastClue.sln --configuration Release --no-restore
dotnet test LastClue.sln --configuration Release --no-build
dotnet run --project src/LastClue.Web --launch-profile http
```

Use the URL printed by the host. For the HTTPS profile, configure a trusted local development certificate on your own machine with `dotnet dev-certs https --trust`, then select the `https` launch profile. Production traffic will use HTTPS.

## Package versions

| Package | Version |
| --- | --- |
| Microsoft.NET.Test.Sdk | 17.14.1 |
| xunit | 2.9.3 |
| xunit.runner.visualstudio | 3.1.4 |
| coverlet.collector | 6.0.4 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.12 |
| Microsoft.Playwright | 1.64.1 |

`Directory.Packages.props` owns the versions. Lock files are committed. The current test suite contains ten rule cases and three web integration tests. Infrastructure and UI test projects have no feature tests yet; browser installation and database fixtures will accompany their first implementation tasks.

## Future configuration

| Setting | Purpose | Local storage |
| --- | --- | --- |
| ConnectionStrings:Default | SQL Server connection | User Secrets |
| AiProvider:ApiKey | Server-side provider authentication | User Secrets |
| AiProvider:Model | Selected provider model | Non-secret configuration |
| AiProvider:Enabled | Permit AI requests or use fallback only | Non-secret configuration |
| AppOptions:AllowedOrigin | Browser application origin | Environment configuration |

The key names are the intended contracts; the foundation does not yet bind them. Never commit real values. When implemented, AI unavailability must preserve fallback-only startup. Missing SQL or incomplete reviewed case coverage must fail readiness.

## Reference documentation

The web integration tests use [WebApplicationFactory](https://learn.microsoft.com/aspnet/core/test/integration-tests?view=aspnetcore-10.0). CI reads the SDK from [global.json through setup-dotnet](https://github.com/actions/setup-dotnet).
