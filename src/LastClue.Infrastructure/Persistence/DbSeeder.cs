using LastClue.Application.Domain;
using LastClue.Application.Exceptions;
using LastClue.Application.Settings;
using Microsoft.EntityFrameworkCore;

namespace LastClue.Infrastructure.Persistence;

public sealed class DbSeeder(AppDbContext database)
{
    public async Task<bool> SeedAsync(string changedBy, CancellationToken cancellationToken = default)
    {
        var settings = SettingsValidator.Parse(SettingsDefaults.Json);
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        // Lock the version key, including the gap when version one does not exist.
        var exists = await database.SettingsVersions
            .FromSql($"SELECT * FROM [SettingsVersions] WITH (UPDLOCK, HOLDLOCK) WHERE [VersionNo] = {1}")
            .AnyAsync(cancellationToken);
        if (exists)
            return false;

        if (!await database.Users.AnyAsync(user => user.Id == changedBy, cancellationToken))
            throw new ValidationException("The settings publisher must be an existing user.");

        var versionId = Guid.NewGuid();
        database.SettingsVersions.Add(new SettingsVersion
        {
            Id = versionId,
            VersionNo = 1,
            ValuesJson = SettingsDefaults.Json,
            ChangedBy = changedBy,
            PublishedAt = DateTime.UtcNow
        });

        database.RankDefinitions.AddRange(settings.Ranks.Select(rank => new RankDefinition
        {
            SettingsVersionId = versionId,
            Code = rank.Code,
            Name = rank.Name,
            MinPoints = rank.MinPoints
        }));
        database.AchievementDefinitions.AddRange(settings.Achievements.Select(achievement => new AchievementDefinition
        {
            SettingsVersionId = versionId,
            Code = achievement.Code,
            Name = achievement.Name,
            CriteriaJson = SettingsValidator.SerializeCriteria(achievement.Criteria)
        }));

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }
}
