using Microsoft.AspNetCore.Mvc.Testing;

namespace LastClue.Web.IntegrationTests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string ConnectionVariable = "LAST_CLUE_TEST_SQLSERVER";

    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionVariable)))
            Skip = $"Set {ConnectionVariable} to run this test.";
    }
}
