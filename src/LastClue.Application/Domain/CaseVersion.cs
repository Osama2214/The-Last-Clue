namespace LastClue.Application.Domain;

/// <summary>A reviewable revision of a fallback case family.</summary>
public sealed class CaseVersion
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public int VersionNo { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
