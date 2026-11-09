using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;
using LastClue.Application.Security;

namespace LastClue.Application.Tests;

public sealed class GameAuthorizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Blocked_accounts_cannot_enter_rooms(bool blocked)
    {
        var authorization = new GameAuthorization(new UserRepository(blocked));
        if (blocked)
            await Assert.ThrowsAsync<ForbiddenException>(() => authorization.RequirePlayerAccountAsync("one"));
        else
            await authorization.RequirePlayerAccountAsync("one");
    }

    [Fact]
    public void A_nonmember_cannot_discover_a_game()
    {
        var authorization = new GameAuthorization(new UserRepository(false));
        var game = new Game();
        game.Players.Add(new GamePlayer { UserId = "one" });
        Assert.Throws<NotFoundException>(() => authorization.RequireMember(game, "two"));
        Assert.Throws<NotFoundException>(() => authorization.RequireMember(null, "two"));
    }

    [Theory]
    [InlineData(GameRole.Detective, true, false, false, true)]
    [InlineData(GameRole.Innocent, true, false, false, false)]
    [InlineData(GameRole.Detective, false, false, false, false)]
    [InlineData(GameRole.Detective, true, true, false, false)]
    [InlineData(GameRole.Detective, true, false, true, false)]
    public void Only_an_active_detective_can_perform_a_detective_action(
        GameRole role, bool active, bool arrested, bool departed, bool allowed)
    {
        var authorization = new GameAuthorization(new UserRepository(false));
        var game = new Game { Status = GameStatus.Active };
        var member = new GamePlayer
        {
            Role = role, IsActive = active, IsArrested = arrested,
            DepartedAt = departed ? DateTime.UnixEpoch : null
        };

        void Authorize()
        {
            authorization.RequireGameRole(member, GameRole.Detective);
            authorization.RequireActive(game, member);
        }

        if (allowed)
            Authorize();
        else
            Assert.Throws<ForbiddenException>(Authorize);
    }

    [Fact]
    public void Wrong_phase_stale_version_and_pause_reject_game_actions()
    {
        var authorization = new GameAuthorization(new UserRepository(false));
        var game = new Game { Phase = Phase.Briefing, StateVersion = 10 };
        authorization.RequirePhase(game, Phase.Briefing, 10);
        Assert.Throws<ConflictException>(() => authorization.RequirePhase(game, Phase.Briefing, 9));
        Assert.Throws<ConflictException>(() => authorization.RequirePhase(game, Phase.Accusation, 10));
        game.PauseStartedAt = DateTime.UnixEpoch;
        Assert.Throws<ConflictException>(() => authorization.RequireNotPaused(game));
    }

    private sealed class UserRepository(bool blocked) : IUserRepository
    {
        public Task<UserSummary?> GetUserAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult<UserSummary?>(new(userId, "Test", "seed", blocked));
    }
}
