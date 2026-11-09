using LastClue.Application.Exceptions;
using LastClue.Application.Settings;

namespace LastClue.Application.Tests;

public sealed class SettingsValidatorTests
{
    [Theory]
    [InlineData(-100, "trainee")]
    [InlineData(49, "trainee")]
    [InlineData(50, "investigator")]
    [InlineData(150, "senior-investigator")]
    [InlineData(300, "expert")]
    public void Rank_covers_negative_totals_and_includes_each_threshold(long score, string code)
    {
        Assert.Equal(code, SettingsValidator.Parse(SettingsDefaults.Json).RankFor(score).Code);
    }

    [Theory]
    [InlineData("\"briefing\": 30", "\"briefing\": 0")]
    [InlineData("\"minPoints\": 50", "\"minPoints\": 0")]
    [InlineData("\"criminalWins\"", "\"unknownMetric\"")]
    [InlineData("\"question\": 300", "\"question\": 301")]
    public void Rejects_invalid_settings(string oldValue, string invalidValue)
    {
        var json = SettingsDefaults.Json.Replace(oldValue, invalidValue, StringComparison.Ordinal);
        Assert.Throws<ValidationException>(() => SettingsValidator.Parse(json));
    }

    [Fact]
    public void Rejects_malformed_json()
    {
        Assert.Throws<ValidationException>(() => SettingsValidator.Parse("{"));
    }
}
