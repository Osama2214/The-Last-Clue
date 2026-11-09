using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Tests;

public sealed class CoreSqlModelTests
{
    [Fact]
    public void Core_schema_preserves_history_and_enforces_room_and_game_uniqueness()
    {
        using var database = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer()
            .Options);

        var sql = database.Database.GenerateCreateScript();

        Assert.Contains("[CreatedAt] datetime2(3)", sql);
        Assert.Contains("[RowVersion] rowversion", sql);
        Assert.Contains("PRIMARY KEY NONCLUSTERED ([GameId], [UserId])", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Rooms_Code]", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_RoomMembers_RoomId_UserId]", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_RoomMembers_UserId] ON [RoomMembers] ([UserId]) WHERE [LeftAt] IS NULL", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Games_RoomId_Number]", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Games_RoomId] ON [Games] ([RoomId]) WHERE [IsRunning] = 1", sql);
        Assert.Contains("CREATE UNIQUE INDEX [IX_GamePlayers_GameId_CharacterKey] ON [GamePlayers] ([GameId], [CharacterKey]) WHERE [CharacterKey] IS NOT NULL", sql);
        Assert.Contains("FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId])", sql);
        Assert.Contains("CHECK ([Language] IN ('en', 'ar'))", sql);
        Assert.DoesNotContain("ON DELETE CASCADE", sql);
    }

    [Theory]
    [InlineData(GameStatus.Starting, true)]
    [InlineData(GameStatus.Active, true)]
    [InlineData(GameStatus.Completed, false)]
    [InlineData(GameStatus.Cancelled, false)]
    public void Lifecycle_changes_keep_the_running_slot_in_sync(GameStatus status, bool isRunning)
    {
        var game = new Game { Status = status };

        Assert.Equal(isRunning, game.IsRunning);
    }
}
