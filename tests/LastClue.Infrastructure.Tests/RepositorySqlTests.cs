using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure.Tests;

public sealed class RepositorySqlTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task A_room_and_its_outbox_event_commit_in_the_same_transaction()
    {
        var room = NewRoom();

        await using (var scope = fixture.CreateScope())
        {
            var services = scope.ServiceProvider;
            var unit = services.GetRequiredService<IUnitOfWork>();
            await unit.BeginTransactionAsync();

            services.GetRequiredService<IRoomRepository>().AddRoom(room);
            services.GetRequiredService<IGameRepository>().AddOutboxMessage(new OutboxMessage
            {
                RoomId = room.Id,
                StateVersion = room.StateVersion,
                Audience = "Public",
                EventType = "RoomUpdated",
                PayloadJson = "{}",
                CreatedAt = DateTime.UtcNow
            });

            await unit.SaveChangesAsync();
            await unit.CommitAsync();
        }

        await using var reader = fixture.CreateScope();
        var loaded = await reader.ServiceProvider.GetRequiredService<IRoomRepository>().GetRoomByCodeAsync(room.Code);
        Assert.Equal(room.Id, loaded?.Id);

        var database = reader.ServiceProvider.GetRequiredService<AppDbContext>();
        var message = await database.OutboxMessages.SingleAsync(item => item.RoomId == room.Id);
        Assert.Equal(loaded!.StateVersion, message.StateVersion);
    }

    [SqlServerFact]
    public async Task A_rolled_back_room_is_not_visible_or_left_tracked()
    {
        await using var scope = fixture.CreateScope();
        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var rooms = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
        var room = NewRoom();

        await unit.BeginTransactionAsync();
        rooms.AddRoom(room);
        await unit.SaveChangesAsync();
        await unit.RollbackAsync();

        Assert.Null(await rooms.GetRoomByCodeAsync(room.Code));
        Assert.Empty(scope.ServiceProvider.GetRequiredService<AppDbContext>().ChangeTracker.Entries());
    }

    [SqlServerFact]
    public async Task A_stale_room_version_is_reported_as_a_domain_conflict()
    {
        var room = NewRoom();
        await SaveRoomAsync(room);

        await using var first = fixture.CreateScope();
        await using var second = fixture.CreateScope();
        var current = await first.ServiceProvider.GetRequiredService<IRoomRepository>().GetRoomByCodeAsync(room.Code);
        var stale = await second.ServiceProvider.GetRequiredService<IRoomRepository>().GetRoomByCodeAsync(room.Code);

        current!.StateVersion = 2;
        await first.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();

        stale!.StateVersion = 3;
        var error = await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => second.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync());

        Assert.Null(error.InnerException);
        Assert.Equal(2, (await first.ServiceProvider.GetRequiredService<AppDbContext>()
            .Rooms.AsNoTracking().SingleAsync(item => item.Id == room.Id)).StateVersion);
    }

    [SqlServerFact]
    public async Task Filtered_unique_membership_allows_history_and_rejects_a_second_active_room()
    {
        var user = NewUser();
        var first = NewRoom();
        var second = NewRoom();
        await using (var scope = fixture.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rooms = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
            database.Users.Add(user);
            rooms.AddRoom(first);
            rooms.AddRoom(second);
            rooms.AddRoomMember(NewMember(first.Id, user.Id, DateTime.UtcNow));
            rooms.AddRoomMember(NewMember(second.Id, user.Id, null));
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }

        await using var writer = fixture.CreateScope();
        var repository = writer.ServiceProvider.GetRequiredService<IRoomRepository>();
        var historical = await repository.GetRoomByCodeAsync(first.Code);
        Assert.NotNull(Assert.Single(historical!.Members).LeftAt);
        historical.Members.Single().LeftAt = null;

        var error = await Assert.ThrowsAsync<DuplicateException>(
            () => writer.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync());
        Assert.Null(error.InnerException);
    }

    [SqlServerFact]
    public async Task A_game_write_lock_blocks_a_second_writer_until_transaction_rollback()
    {
        var room = NewRoom();
        var gameId = Guid.NewGuid();
        var user = NewUser();
        await using (var setup = fixture.CreateScope())
        {
            var services = setup.ServiceProvider;
            var database = services.GetRequiredService<AppDbContext>();
            var settings = new SettingsVersion
            {
                Id = Guid.NewGuid(), VersionNo = 1, ChangedBy = user.Id,
                PublishedAt = DateTime.UtcNow, ValuesJson = "{}"
            };
            database.Users.Add(user);
            database.SettingsVersions.Add(settings);
            services.GetRequiredService<IRoomRepository>().AddRoom(room);
            services.GetRequiredService<IGameRepository>().AddGame(new Game
            {
                Id = gameId, RoomId = room.Id, Number = 1, SettingsVersionId = settings.Id,
                SettingsJson = "{}", CreatedAt = DateTime.UtcNow
            });
            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }

        await using var first = fixture.CreateScope();
        await using var second = fixture.CreateScope();
        var firstUnit = first.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var secondUnit = second.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var firstGames = first.ServiceProvider.GetRequiredService<IGameRepository>();
        var secondGames = second.ServiceProvider.GetRequiredService<IGameRepository>();

        await firstUnit.BeginTransactionAsync();
        Assert.NotNull(await firstGames.GetGameForUpdateAsync(gameId));
        await secondUnit.BeginTransactionAsync();
        var secondDatabase = second.ServiceProvider.GetRequiredService<AppDbContext>();
        await secondDatabase.Database.ExecuteSqlRawAsync("SET LOCK_TIMEOUT 1000");

        var error = await Assert.ThrowsAsync<SqlException>(() => secondGames.GetGameForUpdateAsync(gameId));
        Assert.Equal(1222, error.Number);

        await firstUnit.RollbackAsync();
        Assert.NotNull(await secondGames.GetGameForUpdateAsync(gameId));
        await secondUnit.RollbackAsync();
    }

    private async Task SaveRoomAsync(Room room)
    {
        await using var scope = fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IRoomRepository>().AddRoom(room);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
    }

    private static Room NewRoom() => new()
    {
        Id = Guid.NewGuid(),
        Code = Guid.NewGuid().ToString("N")[..10],
        Language = "en",
        CreatedAt = DateTime.UtcNow,
        StateVersion = 1
    };

    private static AppUser NewUser() => new() { DisplayName = "Test player", CreatedAt = DateTime.UtcNow };

    private static RoomMember NewMember(Guid roomId, string userId, DateTime? leftAt) => new()
    {
        RoomId = roomId,
        UserId = userId,
        JoinedAt = DateTime.UtcNow.AddMinutes(-1),
        LeftAt = leftAt
    };
}
