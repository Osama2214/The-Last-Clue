using System.Text.Json;
using System.Text.Json.Serialization;
using LastClue.Application.Exceptions;

namespace LastClue.Application.Settings;

public static class SettingsValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectRequiredConstructorParameters = true,
        RespectNullableAnnotations = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static GameSettings Parse(string json)
    {
        GameSettings settings;
        try
        {
            settings = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions)
                ?? throw new ValidationException("Settings must be a JSON object.");
        }
        catch (JsonException)
        {
            throw new ValidationException("Settings JSON does not match the required fields.");
        }

        var phases = settings.PhaseSeconds;
        if (phases.Briefing <= 0 || phases.Statements <= 0 || phases.Questioning <= 0
            || phases.ClueRelease <= 0 || phases.Accusation <= 0)
            throw new ValidationException("Phase durations must be positive.");

        var limits = settings.TextLimits;
        if (limits.Statement is < 1 or > 1000 || limits.Question is < 1 or > 300
            || limits.Answer is < 1 or > 1000)
            throw new ValidationException("Text limits must fit the maximum sizes for statements, questions and answers.");

        if (settings.Ranks.Count == 0)
            throw new ValidationException("At least one rank is required.");

        var rankCodes = new HashSet<string>(StringComparer.Ordinal);
        int? previousThreshold = null;
        foreach (var rank in settings.Ranks)
        {
            if (rank is null || !ValidDefinition(rank.Code, rank.Name) || !rankCodes.Add(rank.Code))
                throw new ValidationException("Ranks require unique codes and valid names.");
            if (rank.MinPoints <= previousThreshold)
                throw new ValidationException("Rank thresholds must be strictly ascending.");
            previousThreshold = rank.MinPoints;
        }

        var achievementCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var achievement in settings.Achievements)
        {
            if (achievement is null || !ValidDefinition(achievement.Code, achievement.Name)
                || !achievementCodes.Add(achievement.Code))
                throw new ValidationException("Achievements require unique codes and valid names.");
            if (achievement.Criteria.Minimum <= 0 || achievement.Criteria.Metric is not
                ("completedGames" or "detectiveWins" or "criminalWins"))
                throw new ValidationException("Achievement criteria require a supported metric and a positive target.");
        }

        return settings;
    }

    public static string SerializeCriteria(AchievementCriteria criteria) =>
        JsonSerializer.Serialize(criteria, JsonOptions);

    private static bool ValidDefinition(string code, string name) =>
        code.Length is >= 1 and <= 40
        && code.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-')
        && !string.IsNullOrWhiteSpace(name)
        && name.Length <= 80;
}
