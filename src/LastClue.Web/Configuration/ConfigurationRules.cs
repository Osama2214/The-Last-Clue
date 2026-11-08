using System.Data.Common;

namespace LastClue.Web.Configuration;

internal static class ConfigurationRules
{
    public static bool IsOriginValid(AppOptions options)
    {
        return Uri.TryCreate(options.AllowedOrigin, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host)
            && uri.Host != "*"
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.AbsolutePath == "/"
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment);
    }

    public static bool AreLogLevelsValid(LoggingConfigurationOptions options)
    {
        var names = Enum.GetNames<LogLevel>();
        return options.LogLevel.Values.All(value => names.Contains(value, StringComparer.OrdinalIgnoreCase));
    }

    public static string[] CoreErrors(DatabaseOptions options)
    {
        const string key = "ConnectionStrings:Default";
        if (string.IsNullOrWhiteSpace(options.Default))
        {
            return [key];
        }

        try
        {
            var connection = new DbConnectionStringBuilder { ConnectionString = options.Default };
            var serverPresent = HasValue(connection, ["Server", "Data Source", "Address", "Addr", "Network Address"]);
            var databasePresent = HasValue(connection, ["Database", "Initial Catalog"]);
            return serverPresent && databasePresent ? [] : [key];
        }
        catch (ArgumentException)
        {
            // Parser messages can contain parts of the connection string.
            return [key];
        }
    }

    private static bool HasValue(DbConnectionStringBuilder connection, string[] keys)
    {
        return keys.Any(key => connection.TryGetValue(key, out var value)
            && !string.IsNullOrWhiteSpace(Convert.ToString(value)));
    }
}
