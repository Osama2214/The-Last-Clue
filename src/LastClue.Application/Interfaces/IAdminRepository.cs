using LastClue.Application.Domain;

namespace LastClue.Application.Interfaces;

public interface IAdminRepository
{
    /// <summary>Reads recent audit rows for the administration service, newest first.</summary>
    Task<IReadOnlyList<AdminAudit>> GetRecentAuditAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>Tracks an administrative audit record for insertion.</summary>
    void AddAuditEntry(AdminAudit audit);
}
