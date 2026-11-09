namespace LastClue.Application.Interfaces;

public interface IUnitOfWork
{
    /// <summary>Starts a transaction shared by the repositories in this scope.</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Flushes tracked changes and translates database write conflicts.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the active transaction after its changes have been saved.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the active transaction and clears its tracked changes.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
