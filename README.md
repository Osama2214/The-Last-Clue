# The Last Clue

The Last Clue is a multiplayer mystery investigation game for 4–8 players, built with ASP.NET Core MVC, REST APIs and SignalR. A case is loaded before play through AI generation or a reviewed local case library. Players investigate through statements, questions and clues over three rounds.

## Current progress

The repository contains the project documentation, editable diagrams, task tracker and the .NET solution foundation. The web host, health endpoints and initial rule tests run locally. Accounts, persistence, live rooms, case generation and complete gameplay are the next implementation workstreams.

## Team

Cohort: **GIZ5_SWD5_S4** · Digital Egypt Pioneers Initiative · Full Stack .NET

| Member | GitHub |
| --- | --- |
| Mennatullah Mahmoud Fathy Abdullah | [mennatullahmahmoud06-del](https://github.com/mennatullahmahmoud06-del) |
| Mostafa Mohamed Kamel Abdrabouh | [MostafaKamel25](https://github.com/MostafaKamel25) |
| Osama Ahmed Abdelaziz | [Osama2214](https://github.com/Osama2214) |
| Adham Mohamed Ahmed Sayed | [2-dhamm](https://github.com/2-dhamm) |

## Run locally

Install the .NET SDK selected in `global.json` (**10.0.401**), then run:

```sh
git clone https://github.com/Osama2214/The-Last-Clue.git
cd The-Last-Clue
dotnet restore LastClue.sln --locked-mode
dotnet build LastClue.sln --configuration Release --no-restore
dotnet test LastClue.sln --configuration Release --no-build
dotnet run --project src/LastClue.Web --launch-profile http
```

Open the HTTP address printed by the application. The current foundation does not need a database or AI key. `/health` returns 200. `/health/ready` returns 503 until real storage, configuration and reviewed case coverage are implemented. See the [setup guide](docs/setup.md) for details.

## Run with Docker

With Docker Desktop running, execute from the repository root:

```sh
docker compose up --build --detach --wait
```

Open `http://localhost:8080`. See [Docker setup](docs/docker.md) for logs, stopping, port changes and the current health responses.

## Solution

```text
src/
  LastClue.Web/             MVC pages, REST endpoints, SignalR and composition
  LastClue.Application/     Domain rules, services, DTOs and interfaces
  LastClue.Infrastructure/  Persistence, repositories and provider adapters
  LastClue.Workers/         Generation, timers and durable event delivery
tests/
  LastClue.Application.Tests/
  LastClue.Infrastructure.Tests/
  LastClue.Web.IntegrationTests/
  LastClue.UI.Tests/
docs/                      Planning, requirements, design and implementation guides
```

Application has no reference to Web or Infrastructure. Infrastructure and Workers reference Application. Web composes the implementation projects. Database and UI test projects are prepared; their feature tests will be added with the related implementation.

## Game rules

- 4–5 players have one detective; 6–8 players have two. Exactly one suspect is the criminal.
- Three rounds follow Briefing, Statements, Questioning, Clue Release and Accusation.
- At most two accepted accusations per game and one per round. Two detectives must submit matching final votes.
- A criminal arrest ends the game. Two wrong arrests, or round three ending without a catch, give the criminal victory.
- The first connection loss has a 60-second grace period with a shared pause. Essential-player departure cancels without points or solution disclosure.
- Five reviewed fallback case families, each supporting five player counts in both languages, keep startup independent of AI availability. These 50 variants still need to be authored and reviewed.

## Documentation and work tracking

Start with the [documentation index](docs/README.md). The combined documents are available alongside their component files:

- [Planning and management](docs/planning/00_Complete_Phase_1_Project_Planning_and_Management.docx)
- [Requirements](docs/requirements/00_Complete_Requirements_Gathering.docx)
- [Analysis and design](docs/analysis-and-design/00_Complete_System_Analysis_and_Design.docx)
- [Task tracker](docs/tracking/Taska.xlsx)

## Contribution workflow

Work on a short-lived feature branch and open a pull request to `main`. The `Build and test` check and another member's review are required before merging. See [CONTRIBUTING.md](CONTRIBUTING.md).

The CI workflow restores locked dependencies, builds the solution, runs tests with coverage collection and publishes a downloadable application artifact for `main`. Hosting deployment and the Services coverage threshold will be added with the persistence and release work.
