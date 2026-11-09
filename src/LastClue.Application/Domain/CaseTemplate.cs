namespace LastClue.Application.Domain;

/// <summary>The stable identity of a fallback case family.</summary>
public sealed class CaseTemplate
{
    public Guid Id { get; set; }
    /// <summary>The stable key shared by every version of the case family.</summary>
    public string CaseKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
