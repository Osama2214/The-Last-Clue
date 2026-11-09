using LastClue.Application.Settings;
using LastClue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Infrastructure.Tests;

public sealed class SettingsSeedSqlTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Concurrent_and_repeated_seeds_create_one_complete_settings_version()
    {
        var publisher = new AppUser { DisplayName = "Settings publisher", CreatedAt = DateTime.UtcNow };
        await using (var setup = fixture.CreateScope())
        {
            var database = setup.ServiceProvider.GetRequiredService<AppDbContext>();
            database.Users.Add(publisher);
            await database.SaveChangesAsync();
        }

        var results = await Task.WhenAll(SeedAsync(publisher.Id), SeedAsync(publisher.Id));
        Assert.Single(results, created => created);
        Assert.False(await SeedAsync(publisher.Id));

        await using var reader = fixture.CreateScope();
        var stored = reader.ServiceProvider.GetRequiredService<AppDbContext>();
        var version = await stored.SettingsVersions.SingleAsync();
        Assert.Equal(1, version.VersionNo);
        Assert.Equal(publisher.Id, version.ChangedBy);
        Assert.Equal("trainee", SettingsValidator.Parse(version.ValuesJson).RankFor(-21).Code);
        Assert.Equal(new[] { 0, 50, 150, 300 }, await stored.RankDefinitions
            .Where(rank => rank.SettingsVersionId == version.Id)
            .OrderBy(rank => rank.MinPoints).Select(rank => rank.MinPoints).ToArrayAsync());
        Assert.Equal(new[] { "first-case", "first-criminal-win", "first-detective-win" },
            await stored.AchievementDefinitions.Where(achievement => achievement.SettingsVersionId == version.Id)
                .OrderBy(achievement => achievement.Code).Select(achievement => achievement.Code).ToArrayAsync());
    }

    private async Task<bool> SeedAsync(string publisher)
    {
        await using var scope = fixture.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync(publisher);
    }
}
