namespace LastClue.Web.Avatars;

public interface IAvatarService
{
    string? GetUrl(string seed);
}

internal sealed class DiceBearAvatarService : IAvatarService
{
    public string? GetUrl(string seed) => Guid.TryParseExact(seed, "N", out var parsed)
        ? $"https://api.dicebear.com/10.x/shapes/svg?seed={parsed:N}" : null;
}
