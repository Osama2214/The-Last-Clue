using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class ActiveMembershipConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_a_second_active_membership_for_the_same_user()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (first, second, player) = await CreateTwoGamesAsync(database);
        database.RoomMembers.Add(new RoomMember { RoomId = first.RoomId, UserId = player.Id, JoinedAt = Now });
        await database.SaveChangesAsync();
        database.RoomMembers.Add(new RoomMember { RoomId = second.RoomId, UserId = player.Id, JoinedAt = Now });
        await RejectAsync(database, "IX_RoomMembers_UserId", 2601, 2627);
    }
}
