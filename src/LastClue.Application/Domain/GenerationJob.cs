namespace LastClue.Application.Domain;

/// <summary>A durable request to prepare a case for a game.</summary>
public sealed class GenerationJob
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid RequestId { get; set; }
    public JobStatus Status { get; set; }
    /// <summary>The overall deadline for AI generation.</summary>
    public DateTime DeadlineAt { get; set; }
    /// <summary>The UTC time at which another worker may reclaim this job.</summary>
    public DateTime? LeaseUntil { get; set; }
    public int Attempts { get; set; }
    public string? LastErrorCode { get; set; }
}
