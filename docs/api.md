# API implementation status

The application exposes its home page and the operational probes `/health` and `/health/ready`. Account, room, game and administration endpoints are not implemented yet.

The API foundation provides `ApiControllerBase` at `/api/v1/[controller]`, domain error mapping, safe JSON errors, automatic input-validation responses, correlation IDs and the application-cookie challenge/forbid configuration. New business controllers use this foundation. Identity will register the actual cookie handler and account endpoints when the accounts task is implemented.

See the [error contract and code catalog](api-errors.md) and [request logging policy](logging.md). Health probes keep their status-only JSON bodies. Diagnostic and sign-in routes used by the tests are registered in the test host only and are not published.

The intended SignalR path is `/hubs/game`. The [API contracts and testing plan](analysis-and-design/06_API_Contracts_and_Testing_Plan.docx) defines the planned business requests, responses, audiences and acceptance cases. Add working examples as those endpoints are implemented.
