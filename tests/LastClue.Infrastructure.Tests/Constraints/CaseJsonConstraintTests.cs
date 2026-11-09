using LastClue.Application.Domain;
using LastClue.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using static LastClue.Infrastructure.Tests.ConstraintTestData;

namespace LastClue.Infrastructure.Tests;

public sealed class CaseJsonConstraintTests(SqlServerRepositoryFixture fixture)
    : IClassFixture<SqlServerRepositoryFixture>
{
    [SqlServerFact]
    public async Task Rejects_invalid_case_variant_json()
    {
        await using var scope = fixture.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var version = await CreatePublishedVersionAsync(database);
        database.CaseVariants.Add(new CaseVariant { CaseVersionId = version.Id, Language = "en", PlayerCount = 4, PayloadJson = "{invalid" });
        await RejectAsync(database, "CK_CaseVariants_PayloadJson", 547);
    }
}
