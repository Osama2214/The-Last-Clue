namespace LastClue.Application.Interfaces;

public sealed record UserSummary(string Id, string DisplayName, string AvatarSeed, bool IsBlocked);

public interface IUserRepository
{
    /// <summary>Reads a user's profile and access status without Identity credentials or tokens.</summary>
    Task<UserSummary?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
}
