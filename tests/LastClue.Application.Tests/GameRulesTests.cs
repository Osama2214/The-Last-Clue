using LastClue.Application.Domain;

namespace LastClue.Application.Tests;

public sealed class GameRulesTests
{
    [Theory]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    [InlineData(6, 2)]
    [InlineData(7, 2)]
    [InlineData(8, 2)]
    public void Supported_game_sizes_have_the_agreed_detective_count(int players, int expected)
        => Assert.Equal(expected, GameRules.DetectiveCount(players));

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(9)]
    [InlineData(int.MaxValue)]
    public void Unsupported_game_sizes_are_rejected(int players)
        => Assert.Throws<ArgumentOutOfRangeException>(() => GameRules.DetectiveCount(players));
}
