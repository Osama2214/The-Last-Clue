using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class PublishedCaseConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_a_second_published_version_of_the_same_template()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var version = await CreatePublishedVersionAsync(database);
        database.CaseVersions.Add(new CaseVersion { TemplateId = version.TemplateId, VersionNo = 2, ReviewStatus = ReviewStatus.Published });
        await RejectAsync(database, "IX_CaseVersions_TemplateId", 2601, 2627);
    }
}
