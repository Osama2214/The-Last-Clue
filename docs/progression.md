# Default settings and progression

Settings version 1 uses the phase durations, text limits and scoring in the [functional requirements](requirements/03_Functional_Requirements.docx).

| Phase | Seconds |
| --- | ---: |
| Briefing | 30 |
| Statements | 90 |
| Questioning | 120 |
| Clue release | 30 |
| Accusation | 45 |

The maximum lengths are 1,000 characters for statements, 300 for questions and 1,000 for answers.

| Event | Detectives | Criminal | Innocents |
| --- | --- | --- | --- |
| Criminal caught | +10 each, plus +5 in round 1 or 2 | −5 | +3 for each active innocent |
| Wrong arrest | −8 each | +10 | −5 for the arrested innocent |
| Criminal escapes | −5 each | +15 | 0 |
| Game cancelled | 0 | 0 | 0 |

| Rank | Stable code | Minimum points |
| --- | --- | ---: |
| Trainee | trainee | 0 |
| Investigator | investigator | 50 |
| Senior Investigator | senior-investigator | 150 |
| Expert | expert | 300 |

Trainee also covers every negative total. Rank uses the highest threshold reached.

| Achievement | Stable code | Criteria |
| --- | --- | --- |
| First Case | first-case | Complete one game |
| First Detective Win | first-detective-win | Win one game as a detective |
| First Criminal Win | first-criminal-win | Win one game as the criminal |

After applying the database migration, configure the app's database connection and use the ID of an existing user as the publisher.

```powershell
dotnet run --project src/LastClue.Web -- --seed-settings "<existing-user-id>"
```

The command creates version 1 and its definitions in one transaction. Reruns keep the existing version. Admin changes will publish a new version through the settings endpoint when it is implemented. Games use the settings version they started with.
