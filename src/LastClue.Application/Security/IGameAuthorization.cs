using LastClue.Application.Domain;

namespace LastClue.Application.Security;

public interface IGameAuthorization
{
    Task RequirePlayerAccountAsync(string userId, CancellationToken cancellationToken = default);
    GamePlayer RequireMember(Game? game, string userId);
    void RequireGameRole(GamePlayer member, GameRole requiredRole);
    void RequireActive(Game game, GamePlayer member);
    void RequirePhase(Game game, Phase requiredPhase, long expectedStateVersion);
    void RequireNotPaused(Game game);
}
