# API implementation status

The foundation currently exposes the home page and the two operational probes `/health` and `/health/ready`. Account, room, game and administration endpoints have not been implemented yet.

The intended REST base path is `/api/v1`, and the intended SignalR path is `/hubs/game`. The [API contracts and testing plan](analysis-and-design/06_API_Contracts_and_Testing_Plan.docx) defines the requests, responses, audiences and acceptance cases for the implementation tasks. Update this guide with working examples as each endpoint is completed.
