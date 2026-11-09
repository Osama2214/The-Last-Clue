using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class UserRepository(AppDbContext database) : IUserRepository
{
    public Task<UserSummary?> GetUserAsync(string userId, CancellationToken cancellationToken) =>
        database.Users.Where(user => user.Id == userId)
            .Select(user => new UserSummary(user.Id, user.DisplayName, user.AvatarSeed, user.IsBlocked))
            .SingleOrDefaultAsync(cancellationToken);
}
