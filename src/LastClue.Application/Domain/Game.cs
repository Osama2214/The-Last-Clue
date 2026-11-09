namespace LastClue.Application.Domain;

/// <summary>The authoritative state of one playthrough.</summary>
public sealed class Game
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public int Number { get; set; }
    public byte Round { get; set; }
    public Phase Phase { get; set; }
    public DateTime? PhaseEndsAt { get; set; }
    /// <summary>The time at which the current phase timer was paused.</summary>
    public DateTime? PauseStartedAt { get; set; }
    /// <summary>The number of resolved accusations consumed by this game.</summary>
    public byte AccusationsUsed { get; set; }
    public Guid SettingsVersionId { get; set; }
    /// <summary>The settings snapshot captured when the game starts.</summary>
    public string SettingsJson { get; set; } = string.Empty;
    /// <summary>The committed version clients use to order state updates.</summary>
    public long StateVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Outcome? Outcome { get; set; }
    public string? CancellationReason { get; set; }
    /// <summary>The database token used to detect conflicting writes.</summary>
    public byte[] RowVersion { get; set; } = [];

    private GameStatus _status;

    /// <summary>The lifecycle state. Changing it keeps the running flag in sync.</summary>
    public GameStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            IsRunning = value is GameStatus.Starting or GameStatus.Active;
        }
    }

    /// <summary>Whether the game occupies its room's single running game slot.</summary>
    public bool IsRunning { get; private set; } = true;

    public ICollection<GamePlayer> Players { get; } = new List<GamePlayer>();
    public ICollection<Statement> Statements { get; } = new List<Statement>();
    public ICollection<Question> Questions { get; } = new List<Question>();
    public ICollection<AccusationVote> Votes { get; } = new List<AccusationVote>();
    public ICollection<Arrest> Arrests { get; } = new List<Arrest>();
    public ICollection<GameClue> Clues { get; } = new List<GameClue>();
}
