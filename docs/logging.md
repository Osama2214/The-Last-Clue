# Request logging

The Web host writes structured JSON console logs. Every request receives a correlation scope. API failures return that same identifier so a client can report an error without receiving internal details.

## Recorded fields

`ApiRequestCompleted` records the normalized HTTP method, declared route template, response status, elapsed milliseconds, correlation ID, authenticated account ID when available, and whether the connection was aborted.

The route is the server's endpoint template, such as `api/v1/games/{gameId}`, not the submitted path or route values. Unmatched requests use `<unmatched>`. Unknown method tokens use `OTHER`. The account field accepts authenticated UUID identifiers only; email, display name and other claims are omitted.

`ApiFailure` records the correlation ID, account ID, exception type and server stack trace for unexpected failures. Expected domain errors already have public error codes and appear in the completion event. An aborted response keeps its already-started HTTP status in diagnostics and is marked aborted; the application cannot rewrite a status already sent to the client.

## Excluded data

Do not log request or response bodies, query strings, raw paths or route values, headers, cookies, passwords, provider keys, connection strings, private scripts, clue payloads or hidden solutions. Exception messages, inner-exception details and `Exception.Data` can also contain this information. The failure event deliberately records type and stack trace without passing the raw exception object to a provider.

ASP.NET Core HTTP, routing, authentication, MVC and exception diagnostics are disabled through the `Microsoft.AspNetCore` category filter. Those framework events can otherwise include raw request URLs, binding details or exception messages. The application's completion and failure events replace them for APIs. Host lifecycle and the project's configuration diagnostics remain available. Do not re-enable framework request logging or add HTTP body logging without reviewing the disclosure tests.

Logging levels for application categories remain configurable through `Logging:LogLevel`. The checked-in Information level includes completion events. Raising that level to Warning or Error suppresses ordinary completion entries.

## Investigating a failure

Find the client's `X-Correlation-Id` in the JSON console output or container logs. Match the completion event with its failure event, then inspect the exception type and stack frames. Use the status and route template to identify the failing operation. Keep private values out of investigation notes and added logs.

```sh
docker compose logs --follow web
```

The account, case and game request handlers are not implemented yet. Tests register their diagnostic routes only in the test host, trigger real failures with sentinel values, and inspect formatted messages, structured fields, scopes and public responses. Coverage includes Development and Production, authenticated account enrichment, unmatched paths and concurrent correlation IDs. These routes are not published in the application image.
