using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LastClue.Infrastructure.Repositories;

internal sealed class UnitOfWork(AppDbContext database) : IUnitOfWork, IAsyncDisposable
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
            throw new InvalidOperationException("This unit of work already has an active transaction.");

        _transaction = await database.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException sql && IsDuplicate(sql))
        {
            throw new DuplicateException();
        }
        catch (SqlException exception) when (IsDuplicate(exception))
        {
            throw new DuplicateException();
        }
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        var transaction = GetTransaction();
        await transaction.CommitAsync(cancellationToken);
        await DisposeAsync();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        var transaction = GetTransaction();
        await transaction.RollbackAsync(cancellationToken);
        await DisposeAsync();
        database.ChangeTracker.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is null)
            return;

        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private IDbContextTransaction GetTransaction() =>
        _transaction ?? throw new InvalidOperationException("This unit of work has no active transaction.");

    private static bool IsDuplicate(SqlException exception) =>
        exception.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
}
