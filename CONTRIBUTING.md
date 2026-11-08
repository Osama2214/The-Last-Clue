# Contributing

## Branches and reviews

Use branches such as `feature/accounts-registration`, `fix/lobby-readiness` or `docs/case-schema`. Keep one change focused on one task. Open a pull request to `main` with the task ID, the resulting behavior and the checks you ran. Another member reviews it before merge. Keep discussion about implementation in the pull request so the team can follow the decision.

## Before opening a pull request

```sh
dotnet restore LastClue.sln --locked-mode
dotnet build LastClue.sln --configuration Release --no-restore
dotnet test LastClue.sln --configuration Release --no-build
```

Add tests for meaningful behavior and update the related documentation. Check role visibility when changing game state, events or history. Review database migrations before release. Do not commit credentials, local settings, `bin`, `obj` or test output.

## Commits

Use a short description of the change, for example `Add room capacity validation` or `Fix duplicate vote handling`. Configure your own identity on your machine. Pull request authors and reviewers remain responsible for their changes.

## Dependencies

Pin package versions in `Directory.Packages.props`. Commit each affected `packages.lock.json` after a dependency change. Update the actual SDK version in `global.json` deliberately and validate a clean restore before merging.

## Work tracking

Update owners, statuses and notes in the [online task tracker](https://docs.google.com/spreadsheets/d/14sEpuYsHtw5xIbB4qP7JUSKR0zdDnTnz/edit?gid=875743418#gid=875743418). The local workbook is a saved snapshot. `Ready?` is calculated from dependency IDs. Use `In Progress` while implementation or acceptance checks remain, `In Review` while a completed change is being reviewed, and `Done` after the acceptance checks and review pass. Include the task ID and pull request link in the notes.
