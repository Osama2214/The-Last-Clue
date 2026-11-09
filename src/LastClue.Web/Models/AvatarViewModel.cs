using System.Globalization;

namespace LastClue.Web.Models;

public sealed record AvatarViewModel(string DisplayName, string Seed)
{
    public string Initials => string.Concat(DisplayName
        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        .Take(2)
        .Select(part => StringInfo.GetNextTextElement(part))).ToUpperInvariant();
}
