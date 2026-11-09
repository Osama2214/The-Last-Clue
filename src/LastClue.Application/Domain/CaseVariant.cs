namespace LastClue.Application.Domain;

/// <summary>A case revision adapted to one language and player count.</summary>
public sealed class CaseVariant
{
    public Guid Id { get; set; }
    public Guid CaseVersionId { get; set; }
    public byte PlayerCount { get; set; }
    public string Language { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public bool IsValidated { get; set; }
}
