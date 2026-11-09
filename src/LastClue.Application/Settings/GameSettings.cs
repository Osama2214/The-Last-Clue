namespace LastClue.Application.Settings;

public sealed record GameSettings(
    PhaseSeconds PhaseSeconds,
    TextLimits TextLimits,
    ScoringSettings Scoring,
    IReadOnlyList<RankSetting> Ranks,
    IReadOnlyList<AchievementSetting> Achievements)
{
    public RankSetting RankFor(long totalPoints)
    {
        // The lowest configured rank also covers totals below its threshold.
        var rank = Ranks[0];
        foreach (var candidate in Ranks)
        {
            if (totalPoints < candidate.MinPoints)
                break;
            rank = candidate;
        }
        return rank;
    }
}

public sealed record PhaseSeconds(int Briefing, int Statements, int Questioning, int ClueRelease, int Accusation);

public sealed record TextLimits(int Statement, int Question, int Answer);

public sealed record ScoringSettings(
    int DetectiveCatch,
    int DetectiveEarlyCatchBonus,
    int CriminalCaught,
    int ActiveInnocentOnCatch,
    int DetectiveWrongArrest,
    int CriminalWrongArrest,
    int ArrestedInnocent,
    int DetectiveCriminalEscapes,
    int CriminalEscapes);

public sealed record RankSetting(string Code, string Name, int MinPoints);

public sealed record AchievementSetting(string Code, string Name, AchievementCriteria Criteria);

public sealed record AchievementCriteria(string Metric, int Minimum);
