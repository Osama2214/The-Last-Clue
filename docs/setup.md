# Local setup

## Requirements

- Git.
- .NET SDK 10.0.401, pinned in `global.json`; later patches in that feature band are permitted.
- Any editor with C# support.

The foundation runs without SQL Server or an AI provider. Add those prerequisites when the persistence and provider tasks are implemented; do not treat the scaffold as a finished game.

For container-based local development, use the [Docker setup](docker.md).

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

`Directory.Packages.props` owns the versions. Lock files are committed. The current test suite contains ten rule cases and twenty-six web integration tests. Infrastructure and UI test projects have no feature tests yet; browser installation and database fixtures will accompany their first implementation tasks.

## Configuration

The web host binds `DatabaseOptions`, `AiProviderOptions`, `AppOptions` and logging options. Checked-in defaults contain no connection string or provider key.

| Setting | Purpose and default | Storage |
| --- | --- | --- |
| ConnectionStrings:Default | SQL Server connection string; absent by default | User Secrets locally, deployment configuration when hosted |
| AiProvider:Enabled | Allow a generation attempt when configuration permits it; false by default | Non-secret configuration |
| AiProvider:KnownOutage | Select fallback without trying an unavailable provider; false by default | Non-secret configuration |
| AiProvider:ApiKey | Server-side provider authentication | User Secrets or deployment configuration |
| AiProvider:Model | Provider model to request; no model is selected by default | Non-secret configuration |
| AppOptions:AllowedOrigin | Browser application origin; http://localhost:5102 locally | Non-secret configuration |
| Logging:LogLevel | Named logging levels such as Information and Warning | Non-secret configuration |

`AllowedOrigin` must be an HTTP or HTTPS origin, with an optional port, but no credentials, application path, query or fragment. An invalid origin or unsupported logging level prevents startup. This setting does not enable cross-origin requests; the application still uses its same-origin design.

A missing or malformed database connection string keeps the host alive but fails the configuration readiness check. That check verifies connection-string syntax and the presence of a server and database name. It does not connect to SQL Server or validate SQL credentials. The actual database and reviewed-case checks remain part of the persistence and case-library tasks, so aggregate readiness still returns 503.

AI is eligible for an attempt only when it is enabled, no known outage is declared, and both a key and model are present. Otherwise `AiProviderOptions.CanAttemptGeneration` is false and `FallbackReason` identifies disabled generation, a known outage, missing credentials or a missing model. AI configuration alone does not make core readiness fail. Provider authentication failures and the actual fallback loading path will be tested with the case-generation implementation.

## Local secret storage

The web project already defines its User Secrets ID. Set secret values through the .NET User Secrets tool or the editor's secret manager. The secret keys are `ConnectionStrings:Default` and `AiProvider:ApiKey`; keep their values out of tracked configuration, Compose files, commits and logs.

To explicitly select fallback-only configuration:

```sh
dotnet user-secrets set "AiProvider:Enabled" "false" --project src/LastClue.Web
```

Restart the host after changing configuration. In Docker, Compose supplies `AppOptions__AllowedOrigin` using its selected host port. Host User Secrets are not copied into the container; future database and provider integration must supply container secrets at runtime.

Startup diagnostics and the internal configuration check identify required key names only. They never include connection-string values, provider keys or parser exception details. The public probe response remains a minimal status object. Integration tests capture logs and responses to check this behavior.

## Reference documentation

The web integration tests use [WebApplicationFactory](https://learn.microsoft.com/aspnet/core/test/integration-tests?view=aspnetcore-10.0). CI reads the SDK from [global.json through setup-dotnet](https://github.com/actions/setup-dotnet).

Options binding and startup validation follow the [ASP.NET Core options guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0).
