using LastClue.Application.Domain;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class AdminRepository(AppDbContext database) : IAdminRepository
{
    public async Task<IReadOnlyList<AdminAudit>> GetRecentAuditAsync(int limit, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
        return await database.AdminAudit.AsNoTracking()
            .OrderByDescending(entry => entry.OccurredAt)
            .ThenByDescending(entry => entry.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public void AddAuditEntry(AdminAudit audit) => database.AdminAudit.Add(audit);
}
