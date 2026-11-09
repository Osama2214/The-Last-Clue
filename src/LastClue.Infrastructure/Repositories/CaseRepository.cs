using LastClue.Application.Domain;
using LastClue.Application.Interfaces;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Repositories;

internal sealed class CaseRepository(AppDbContext database) : ICaseRepository
{
    public async Task<IReadOnlyList<CaseVariant>> GetPublishedCaseVariantsAsync(
        string language, byte playerCount, CancellationToken cancellationToken)
    {
        var query =
            from variant in database.CaseVariants
            join version in database.CaseVersions on variant.CaseVersionId equals version.Id
            join template in database.CaseTemplates on version.TemplateId equals template.Id
            where template.IsActive && version.ReviewStatus == ReviewStatus.Published
                && variant.IsValidated && variant.Language == language && variant.PlayerCount == playerCount
            orderby template.CaseKey
            select variant;

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    public void AddGameCase(GameCase gameCase) => database.GameCases.Add(gameCase);
}
