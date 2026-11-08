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

Select an owner and status in `docs/tracking/Taska.xlsx`. `Ready?` is calculated from the dependency IDs. Use `In Review` while the pull request is being checked, and `Done` only after the acceptance checks and review pass. Update the tracker through a pull request; avoid several members editing the binary workbook at once.
