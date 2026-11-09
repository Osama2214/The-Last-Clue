namespace LastClue.Application.Domain;

/// <summary>The saved outcome of an idempotent request.</summary>
public sealed class ActionReceipt
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid? GameId { get; set; }
    public Guid RequestId { get; set; }
    /// <summary>The SHA-256 hash of the canonical request payload.</summary>
    public string PayloadHash { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
