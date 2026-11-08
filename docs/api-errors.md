# API error contract

New API controllers derive from `ApiControllerBase`, which supplies `[ApiController]` and the `api/v1/[controller]` route. Account, room, game and administration endpoints remain separate implementation tasks.

Failures under the `/api` path segment return JSON with `code`, a public `message`, and `correlationId`. Responses are not cached. Exception messages, model-binding errors and submitted values are never copied into this response.

```json
{
  "code": "PHASE_CHANGED",
  "message": "The game phase has changed. Reload the current state.",
  "correlationId": "20ea771d-d14c-42ce-9a08-0bcedd1e2b2b",
  "currentStateVersion": 42
}
```

`currentStateVersion` is present only for a stale-phase conflict. The server must verify membership and authorization before revealing an authorized state version. The other conflict codes do not disclose it.

| Code | Status | Meaning |
| --- | --- | --- |
| INVALID_INPUT | 400 | Invalid JSON, model validation or a rejected domain input |
| AUTHENTICATION_REQUIRED | 401 | Authentication is required |
| FORBIDDEN | 403 | The authenticated actor lacks permission |
| NOT_FOUND | 404 | Missing or deliberately hidden resource, or no matching route |
| METHOD_NOT_ALLOWED | 405 | An endpoint returns an unsupported-method response |
| CONFLICT | 409 | A conflict without a more specific domain code |
| PHASE_CHANGED | 409 | The phase/version changed; includes the authorized current version |
| ROOM_FULL | 409 | The room has reached capacity |
| ALREADY_IN_ROOM | 409 | The account already belongs to a room |
| REPLAY_MISMATCH | 409 | A request ID was reused with a different payload |
| PAYLOAD_TOO_LARGE | 413 | The request exceeds an accepted transport limit |
| UNSUPPORTED_MEDIA_TYPE | 415 | The endpoint cannot accept the content type |
| RATE_LIMITED | 429 | Requests exceed a limit; Retry-After is included when known |
| INTERNAL_ERROR | 500 | Unexpected server failure; use the correlation ID for investigation |
| SERVICE_UNAVAILABLE | 503 | A service is temporarily unavailable |
| HTTP_ERROR | Other 4xx | An otherwise unmapped HTTP error |

The domain exception types live in Application and have no dependency on HTTP. The Web layer maps their approved codes to statuses and public messages. A stale-phase exception requires a non-negative current version. Invalid model responses use the same contract instead of exposing framework validation details.

## Correlation IDs

Every response, including web pages and health probes, includes `X-Correlation-Id`. A caller can supply one UUID in standard hyphenated or 32-digit form. It is normalized to hyphenated form. Missing, malformed or repeated values are replaced by a generated UUID. Do not put private data in this header.

API JSON errors include the same identifier. Concurrent requests keep independent identifiers. The header is assigned when the response starts so the correlation value remains authoritative.

## Authentication and response boundaries

The Identity application-cookie options return JSON 401/403 for API challenges and forbids, without a login-page redirect. Ordinary web-page cookie behavior is preserved. The cookie handler, account persistence and real sign-in endpoints will be registered by the accounts task; tests register a real cookie handler only in the test host to verify this integration point.

The API handler is active in Development and Production. It does not turn API failures into an HTML development exception page. The ordinary production MVC error page remains separate. Empty HTTP error responses are wrapped. MVC error results with custom object or HTML content are normalized before execution so they cannot accidentally echo private details. Results already containing the approved ApiError contract are preserved; their messages must remain public and curated. Endpoints that write directly to the response must use the same contract. If a response has already started, an exception aborts the connection instead of appending a misleading JSON body. Client cancellation is not converted into a new server error.

Health probes retain their existing status-only bodies. `/apiary` is outside the API path segment. Business endpoints and the request-rate policy are not introduced by the error-contract task.

References: [ASP.NET Core error handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-10.0), [cookie authentication for API endpoints](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/api-endpoint-auth?view=aspnetcore-10.0).
