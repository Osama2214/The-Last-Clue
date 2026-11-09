# API implementation status

The application exposes its home page and the operational probes `/health` and `/health/ready`. The account endpoints below use Identity cookies.

The API foundation provides `ApiControllerBase` at `/api/v1/[controller]`, domain error mapping, safe JSON errors, automatic input-validation responses, correlation IDs and the application-cookie challenge/forbid configuration. New business controllers use this foundation. Identity will register the actual cookie handler and account endpoints when the accounts task is implemented.

See the [error contract and code catalog](api-errors.md) and [request logging policy](logging.md). Health probes keep their status-only JSON bodies. Diagnostic and sign-in routes used by the tests are registered in the test host only and are not published.

The intended SignalR path is `/hubs/game`. The [API contracts and testing plan](analysis-and-design/06_API_Contracts_and_Testing_Plan.docx) defines the planned business requests, responses, audiences and acceptance cases. Add working examples as those endpoints are implemented.

## Accounts

Browser writes send the token from the `csrf-token` meta tag in the `X-CSRF-TOKEN` header, using the same-origin antiforgery cookie. After login or logout, load a fresh page before sending another write.

| Method and path | Request | Success |
| --- | --- | --- |
| POST /api/v1/auth/register | email, displayName, password | 201 |
| POST /api/v1/auth/login | email, password | 200 and the application cookie |
| POST /api/v1/auth/logout | Authenticated request | 204 and the cookie is cleared |
| GET /api/v1/profiles/me | Authenticated request | 200 with the caller's user ID and email |

Registration may return field errors and returns 409 for a duplicate email. Failed login returns the same 401 code and message for a missing account, blocked account or incorrect password. Signing out invalidates existing cookies for the account.

## Rooms

| Method and path | Request | Success |
| --- | --- | --- |
| POST /api/v1/rooms | language (en or ar) | 201 with a lobby view |
| POST /api/v1/rooms/join | code | 200 with a lobby view |
| GET /api/v1/rooms/{roomId} | Member of the room | 200 with a lobby view |
| PUT /api/v1/rooms/{roomId}/ready | isReady | 200 with the lobby view |
| POST /api/v1/rooms/{roomId}/leave | Member of a lobby | 204 |

Room codes default to six characters from A–Z and 2–9, with I and O excluded. Set `RoomCodeLength` to change the length, up to 12. Join codes are trimmed and case-insensitive. Join and readiness changes require an open lobby, and a room holds up to eight current members. Joining or leaving clears readiness. When a host leaves, the earliest joined connected member becomes host. If no member is connected, the room has no host until an eligible member reconnects. An empty room closes.

Room entry uses `RateLimits:RoomEntry:Permits` and `RateLimits:RoomEntry:WindowSeconds`, defaulting to 10 requests per 60 seconds per account. The separate case-start policy defaults to 3 per 60 seconds and will be attached to the case-start endpoint when that endpoint is added. A rejected rate-limited request returns 429 and `Retry-After`.

## Real-time connection

Players connect to `/hubs/game` with their application cookie. Call `JoinRoom(roomId)` or `JoinGame(gameId)` after connection. Membership is checked before joining `room:{id}` or `game:{id}`. The server assigns `user:{gameId}:{userId}` to eligible players based on the caller's own identity. The client does not provide a user ID or group name. Lobby members count as connected until their last room subscription disconnects. Game subscriptions record individual connections in PlayerConnections and close each record when its connection ends.

Load `lib/signalr/signalr.min.js` before `js/game-connection.js`. The client restores successful subscriptions on reconnect. Lobby mutations save their outbox entries in the same transaction. The dispatcher will deliver those entries in a later task.
