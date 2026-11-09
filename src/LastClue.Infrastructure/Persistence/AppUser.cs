using Microsoft.AspNetCore.Identity;

namespace LastClue.Infrastructure.Persistence;

public sealed class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarSeed { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
