using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class ActionReceiptService(AppDbContext database) : IActionReceiptService
{
    public async Task<ActionReceipt?> TryGetAsync(
        string userId, Guid requestId, string payloadHash, CancellationToken cancellationToken)
    {
        if (database.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Start a transaction before locking an action receipt.");

        var receipt = await database.ActionReceipts
            .FromSql($"""
                SELECT * FROM [ActionReceipts] WITH (UPDLOCK, HOLDLOCK)
                WHERE [UserId] = {userId} AND [RequestId] = {requestId}
                """)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (receipt is not null && !string.Equals(receipt.PayloadHash, payloadHash, StringComparison.Ordinal))
            throw new ConflictException(DomainErrorCode.ReplayMismatch);

        return receipt;
    }

    public void Add(ActionReceipt receipt) => database.ActionReceipts.Add(receipt);
}
