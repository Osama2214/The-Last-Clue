using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure.Tests;

internal static class ConstraintTestData
{
    internal static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal static async Task<(Game First, Game Second, AppUser Player)> CreateTwoGamesAsync(AppDbContext database)
    {
        var player = new AppUser { DisplayName = "Constraint player", CreatedAt = Now };
        var settings = new SettingsVersion
        {
            Id = Guid.NewGuid(),
            VersionNo = (await database.SettingsVersions.MaxAsync(version => (int?)version.VersionNo) ?? 0) + 1,
            ValuesJson = "{}", PublishedAt = Now, ChangedBy = player.Id
        };
        var firstRoom = NewRoom();
        var secondRoom = NewRoom();
        var firstGame = NewGame(firstRoom.Id, settings.Id, 1);
        var secondGame = NewGame(secondRoom.Id, settings.Id, 1);
        database.Users.Add(player);
        database.SettingsVersions.Add(settings);
        database.Rooms.AddRange(firstRoom, secondRoom);
        database.Games.AddRange(firstGame, secondGame);
        database.GamePlayers.AddRange(
            new GamePlayer { GameId = firstGame.Id, UserId = player.Id, IsActive = true },
            new GamePlayer { GameId = secondGame.Id, UserId = player.Id, IsActive = true });
        await database.SaveChangesAsync();
        return (firstGame, secondGame, player);
    }

    internal static Game NewGame(Guid roomId, Guid settingsId, int number) => new()
    {
        Id = Guid.NewGuid(), RoomId = roomId, SettingsVersionId = settingsId, Number = number,
        Status = GameStatus.Starting, SettingsJson = "{}", CreatedAt = Now
    };

    private static Room NewRoom() => new()
    {
        Id = Guid.NewGuid(), Code = Guid.NewGuid().ToString("N")[..10],
        Language = "en", CreatedAt = Now
    };

    internal static async Task<CaseVersion> CreatePublishedVersionAsync(AppDbContext database)
    {
        var template = new CaseTemplate { Id = Guid.NewGuid(), CaseKey = Guid.NewGuid().ToString("N"), Title = "Test case" };
        var version = new CaseVersion
        {
            Id = Guid.NewGuid(), TemplateId = template.Id, VersionNo = 1, ReviewStatus = ReviewStatus.Published
        };
        database.CaseTemplates.Add(template);
        database.CaseVersions.Add(version);
        await database.SaveChangesAsync();
        return version;
    }

    internal static async Task RejectAsync(AppDbContext database, string constraint, params int[] errorNumbers)
    {
        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        var sql = Assert.IsType<SqlException>(failure.InnerException);
        Assert.Contains(sql.Errors.Cast<SqlError>(),
            error => errorNumbers.Contains(error.Number) && error.Message.Contains(constraint, StringComparison.Ordinal));
    }
}
