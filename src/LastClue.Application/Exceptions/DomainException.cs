namespace LastClue.Application.Exceptions;

public enum DomainErrorCode
{
    InvalidInput, Forbidden, NotFound, PhaseChanged, RoomFull, RoomNotOpen, AlreadyInRoom, ReplayMismatch, RateLimited, ConcurrencyConflict, Duplicate
}

public abstract class DomainException : Exception
{
    public DomainErrorCode Code { get; }
    protected DomainException(DomainErrorCode code, string? detail) : base(detail) => Code = code;
}

public sealed class ValidationException(string? detail = null) : DomainException(DomainErrorCode.InvalidInput, detail);
public sealed class ForbiddenException(string? detail = null) : DomainException(DomainErrorCode.Forbidden, detail);
public sealed class NotFoundException(string? detail = null) : DomainException(DomainErrorCode.NotFound, detail);

public class ConflictException : DomainException
{
    public long? CurrentStateVersion { get; }
    public ConflictException(DomainErrorCode code, long? currentStateVersion = null, string? detail = null)
        : base(code, detail)
    {
        if (code is not (DomainErrorCode.PhaseChanged or DomainErrorCode.RoomFull or DomainErrorCode.RoomNotOpen
            or DomainErrorCode.AlreadyInRoom or DomainErrorCode.ReplayMismatch
            or DomainErrorCode.ConcurrencyConflict or DomainErrorCode.Duplicate))
            throw new ArgumentOutOfRangeException(nameof(code));
        if (currentStateVersion < 0 || (code == DomainErrorCode.PhaseChanged && currentStateVersion is null))
            throw new ArgumentOutOfRangeException(nameof(currentStateVersion));
        CurrentStateVersion = currentStateVersion;
    }
}

public sealed class RateLimitedException : DomainException
{
    public int? RetryAfterSeconds { get; }
    public RateLimitedException(int? retryAfterSeconds = null, string? detail = null)
        : base(DomainErrorCode.RateLimited, detail)
    {
        if (retryAfterSeconds < 1) throw new ArgumentOutOfRangeException(nameof(retryAfterSeconds));
        RetryAfterSeconds = retryAfterSeconds;
    }
}

public sealed class ConcurrencyConflictException()
    : ConflictException(DomainErrorCode.ConcurrencyConflict);

public sealed class DuplicateException()
    : ConflictException(DomainErrorCode.Duplicate);
