namespace LastClue.Application.Domain;

/// <summary>A record of an administrative action and its safe metadata.</summary>
public sealed class AdminAudit
{
    public Guid Id { get; set; }
    public string ActorId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    /// <summary>Audit metadata that excludes credentials and private gameplay data.</summary>
    public string SafeDetailsJson { get; set; } = string.Empty;
}
