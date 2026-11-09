using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;

namespace LastClue.Application.Security;

public sealed class GameAuthorization(IUserRepository users) : IGameAuthorization
{
    public async Task RequirePlayerAccountAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (await users.GetUserAsync(userId, cancellationToken) is not { IsBlocked: false })
            throw new ForbiddenException();
    }

    public GamePlayer RequireMember(Game? game, string userId) =>
        game?.Players.SingleOrDefault(member => member.UserId == userId) ?? throw new NotFoundException();

    public void RequireGameRole(GamePlayer member, GameRole requiredRole)
    {
        if (member.Role != requiredRole)
            throw new ForbiddenException();
    }

    public void RequireActive(Game game, GamePlayer member)
    {
        if (!member.IsActive || member.IsArrested || member.DepartedAt is not null)
            throw new ForbiddenException();

        if (game.Status != GameStatus.Active)
            throw new ConflictException(DomainErrorCode.PhaseChanged, game.StateVersion);
    }

    public void RequirePhase(Game game, Phase requiredPhase, long expectedStateVersion)
    {
        if (game.Phase != requiredPhase || game.StateVersion != expectedStateVersion)
            throw new ConflictException(DomainErrorCode.PhaseChanged, game.StateVersion);
    }

    public void RequireNotPaused(Game game)
    {
        if (game.PauseStartedAt is not null)
            throw new ConflictException(DomainErrorCode.PhaseChanged, game.StateVersion);
    }
}
