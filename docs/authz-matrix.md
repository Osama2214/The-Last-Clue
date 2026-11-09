# Authorization

Application cookies authenticate the request. Player and Admin are separate account roles. Room membership and game roles are determined on the server.

| Action | Account | Resource access | Game state |
| --- | --- | --- | --- |
| Read own profile or sign out | Signed in | Own account | Any |
| Create or join a room | Player | Unblocked account with no active membership | Lobby for join |
| Read or change lobby readiness | Player | Current room member | Lobby for writes |
| Submit a statement or answer | Player | Active game member with the required role | Matching phase, unpaused |
| Ask a question or accuse | Player | Active detective in the game | Matching phase, unpaused |
| Manage users or cases | Admin | Admin policy | As required by the operation |

An anonymous API request receives JSON 401. A missing membership receives 404 to hide the resource. A member with the wrong role or an inactive participant attempting a game action receives 403. A stale version, wrong phase or paused game rejects the action with 409. The gameplay rows describe checks to use when the corresponding endpoints are added.
