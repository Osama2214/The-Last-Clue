using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using LastClue.Web.Api;
using Microsoft.AspNetCore.RateLimiting;

namespace LastClue.Web.Configuration;

internal static class RoomRateLimits
{
    public const string Entry = "room-entry";
    public const string CaseStart = "case-start";

    public static IServiceCollection AddRoomRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        var entry = ReadLimit(configuration, "RoomEntry", 10);
        var start = ReadLimit(configuration, "CaseStart", 3);
        return services.AddRateLimiter(options =>
        {
            options.AddPolicy(Entry, context => Partition(context, entry));
            options.AddPolicy(CaseStart, context => Partition(context, start));
            options.OnRejected = async (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, Math.Ceiling(retry.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

                await ApiErrors.WriteAsync(context.HttpContext, StatusCodes.Status429TooManyRequests);
            };
        });
    }

    private static RateLimitPartition<string> Partition(HttpContext context, (int Permits, TimeSpan Window) limit)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? RateLimitPartition.GetNoLimiter(string.Empty)
            : RateLimitPartition.GetFixedWindowLimiter(userId, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit.Permits, Window = limit.Window, QueueLimit = 0
            });
    }

    private static (int Permits, TimeSpan Window) ReadLimit(IConfiguration configuration, string key, int fallback)
    {
        var permits = configuration.GetValue($"RateLimits:{key}:Permits", fallback);
        var seconds = configuration.GetValue($"RateLimits:{key}:WindowSeconds", 60);
        if (permits <= 0 || seconds <= 0)
            throw new InvalidOperationException($"RateLimits:{key} requires positive permits and window seconds.");

        return (permits, TimeSpan.FromSeconds(seconds));
    }
}
