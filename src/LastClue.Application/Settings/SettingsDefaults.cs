namespace LastClue.Application.Settings;

public static class SettingsDefaults
{
    public const string Json = """
        {
          "phaseSeconds": {
            "briefing": 30,
            "statements": 90,
            "questioning": 120,
            "clueRelease": 30,
            "accusation": 45
          },
          "textLimits": {
            "statement": 1000,
            "question": 300,
            "answer": 1000
          },
          "scoring": {
            "detectiveCatch": 10,
            "detectiveEarlyCatchBonus": 5,
            "criminalCaught": -5,
            "activeInnocentOnCatch": 3,
            "detectiveWrongArrest": -8,
            "criminalWrongArrest": 10,
            "arrestedInnocent": -5,
            "detectiveCriminalEscapes": -5,
            "criminalEscapes": 15
          },
          "ranks": [
            {
              "code": "trainee",
              "name": "Trainee",
              "minPoints": 0
            },
            {
              "code": "investigator",
              "name": "Investigator",
              "minPoints": 50
            },
            {
              "code": "senior-investigator",
              "name": "Senior Investigator",
              "minPoints": 150
            },
            {
              "code": "expert",
              "name": "Expert",
              "minPoints": 300
            }
          ],
          "achievements": [
            {
              "code": "first-case",
              "name": "First Case",
              "criteria": {
                "metric": "completedGames",
                "minimum": 1
              }
            },
            {
              "code": "first-detective-win",
              "name": "First Detective Win",
              "criteria": {
                "metric": "detectiveWins",
                "minimum": 1
              }
            },
            {
              "code": "first-criminal-win",
              "name": "First Criminal Win",
              "criteria": {
                "metric": "criminalWins",
                "minimum": 1
              }
            }
          ]
        }
        """;
}
