using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LastClue.Application.Domain;
using LastClue.Application.Rooms;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LastClue.Web.IntegrationTests;

public sealed class RoomTests(AccountTestDatabase database) : IClassFixture<AccountTestDatabase>
{
    [SqlServerFact]
    public async Task Create_retries_a_code_collision_and_blocks_second_room()
    {
        using var factory = database.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRoomCodeGenerator>();
            services.AddSingleton<IRoomCodeGenerator>(new FixedCodes());
        }));
        using var host = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        using var secondHost = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        await SignInPlayerAsync(host);
        await SignInPlayerAsync(secondHost);

        var first = await CreateAsync(host);
        Assert.Equal("AAAAAA", first.Code);
        Assert.Equal(first.Host, Assert.Single(first.Members).UserId);

        var second = await CreateAsync(secondHost);
        Assert.Equal("BBBBBB", second.Code);
        Assert.Equal(second.Host, Assert.Single(second.Members).UserId);
        using var duplicate = await WriteAsync(secondHost, "", new { language = "en" });
        await AssertError(duplicate, HttpStatusCode.Conflict, "ALREADY_IN_ROOM");
    }

    [SqlServerFact]
    public async Task Join_checks_room_status_capacity_and_membership_and_publishes_one_version()
    {
        using var host = database.CreateClient();
        using var player = database.CreateClient();
        await SignInPlayerAsync(host);
        await SignInPlayerAsync(player);
        var room = await CreateAsync(host);

        using var missing = await WriteAsync(player, "/join", new { code = "000000" });
        await AssertError(missing, HttpStatusCode.NotFound, "NOT_FOUND");

        await using var scope = database.Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedRoom = await context.Rooms.SingleAsync(candidate => candidate.Id == room.RoomId);
        var hostMember = await context.RoomMembers.SingleAsync(member => member.RoomId == room.RoomId);
        hostMember.IsReady = true;

        foreach (var status in new[] { RoomStatus.Starting, RoomStatus.Active })
        {
            savedRoom.Status = status;
            await context.SaveChangesAsync();
            using var closed = await WriteAsync(player, "/join", new { code = room.Code });
            await AssertError(closed, HttpStatusCode.Conflict, "ROOM_NOT_OPEN");
        }

        savedRoom.Status = RoomStatus.Lobby;
        await context.SaveChangesAsync();
        using var joined = await WriteAsync(player, "/join", new { code = $" {room.Code.ToLowerInvariant()} " });
        Assert.Equal(HttpStatusCode.OK, joined.StatusCode);
        var result = (await joined.Content.ReadFromJsonAsync<LobbyView>())!;
        Assert.Equal(2, result.Members.Count);
        Assert.All(result.Members, member => Assert.False(member.IsReady));
        Assert.Equal(2, result.StateVersion);
        using var duplicate = await WriteAsync(player, "/join", new { code = room.Code });
        await AssertError(duplicate, HttpStatusCode.Conflict, "ALREADY_IN_ROOM");

        for (var i = 0; i < 6; i++)
        {
            using var other = database.CreateClient();
            await SignInPlayerAsync(other);
            using var added = await WriteAsync(other, "/join", new { code = room.Code });
            Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        }

        using var ninth = database.CreateClient();
        await SignInPlayerAsync(ninth);
        using var full = await WriteAsync(ninth, "/join", new { code = room.Code });
        await AssertError(full, HttpStatusCode.Conflict, "ROOM_FULL");
        Assert.Equal(8, await context.RoomMembers.CountAsync(member => member.RoomId == room.RoomId));
        Assert.Equal(8, await context.OutboxMessages.CountAsync(message => message.RoomId == room.RoomId));
    }

    [SqlServerFact]
    public async Task Join_rate_limit_is_per_account_and_cannot_add_members()
    {
        using var host = database.CreateClient();
        using var admin = database.CreateClient();
        await SignInPlayerAsync(host);
        var room = await CreateAsync(host);
        using var adminLogin = await AccountRequests.PostAsync(admin, "login",
            new { email = AccountTestDatabase.AdminEmail, password = AccountTestDatabase.Password });
        Assert.Equal(HttpStatusCode.OK, adminLogin.StatusCode);
        using var forbidden = await WriteAsync(admin, "/join", new { code = room.Code });
        await AssertError(forbidden, HttpStatusCode.Forbidden, "FORBIDDEN");

        using var player = database.CreateClient();
        await SignInPlayerAsync(player);
        for (var request = 0; request < 10; request++)
        {
            using var miss = await WriteAsync(player, "/join", new { code = "000000" });
            await AssertError(miss, HttpStatusCode.NotFound, "NOT_FOUND");
        }

        using var limited = await WriteAsync(player, "/join", new { code = room.Code });
        await AssertError(limited, HttpStatusCode.TooManyRequests, "RATE_LIMITED");
        await using var scope = database.Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.RoomMembers.CountAsync(member => member.RoomId == room.RoomId));

        using var other = database.CreateClient();
        await SignInPlayerAsync(other);
        using var successful = await WriteAsync(other, "/join", new { code = room.Code });
        Assert.Equal(HttpStatusCode.OK, successful.StatusCode);
    }

    [SqlServerFact]
    public async Task Host_transfer_prefers_the_longest_present_connected_member_and_last_leave_closes_the_room()
    {
        using var host = database.CreateClient();
        using var earliest = database.CreateClient();
        using var latest = database.CreateClient();
        using var outsider = database.CreateClient();
        foreach (var client in new[] { host, earliest, latest, outsider })
            await SignInPlayerAsync(client);

        var room = await CreateAsync(host);
        var path = $"/api/v1/rooms/{room.RoomId:D}";
        using var hidden = await outsider.GetAsync(path);
        await AssertError(hidden, HttpStatusCode.NotFound, "NOT_FOUND");

        using var firstJoin = await WriteAsync(earliest, "/join", new { code = room.Code });
        Assert.Equal(HttpStatusCode.OK, firstJoin.StatusCode);
        using var secondJoin = await WriteAsync(latest, "/join", new { code = room.Code });
        Assert.Equal(HttpStatusCode.OK, secondJoin.StatusCode);
        var members = (await secondJoin.Content.ReadFromJsonAsync<LobbyView>())!.Members;
        var earlyId = members[1].UserId;
        var lateId = members[2].UserId;
        var presence = database.Factory.Services.GetRequiredService<LobbyPresence>();
        presence.Connect(room.RoomId, earlyId, "early-tab");
        presence.Connect(room.RoomId, lateId, "late-tab");

        using var ready = await AccountRequests.WriteAsync(latest, HttpMethod.Put, path + "/ready", new { isReady = true });
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        using var left = await AccountRequests.WriteAsync(host, HttpMethod.Post, path + "/leave");
        Assert.Equal(HttpStatusCode.NoContent, left.StatusCode);
        var view = (await earliest.GetFromJsonAsync<LobbyView>(path))!;
        Assert.Equal(earlyId, view.Host);
        Assert.All(view.Members, member => Assert.False(member.IsReady));

        presence.Disconnect(room.RoomId, lateId, "late-tab");
        using var earlyLeft = await AccountRequests.WriteAsync(earliest, HttpMethod.Post, path + "/leave");
        Assert.Equal(HttpStatusCode.NoContent, earlyLeft.StatusCode);
        Assert.Null((await latest.GetFromJsonAsync<LobbyView>(path))!.Host);

        presence.Connect(room.RoomId, lateId, "late-reconnected");
        await using var scope = database.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IRoomService>().RestoreHostAsync(room.RoomId, lateId, default);
        Assert.Equal(lateId, (await latest.GetFromJsonAsync<LobbyView>(path))!.Host);
        using var lateLeft = await AccountRequests.WriteAsync(latest, HttpMethod.Post, path + "/leave");
        Assert.Equal(HttpStatusCode.NoContent, lateLeft.StatusCode);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var final = await db.Rooms.AsNoTracking().SingleAsync(item => item.Id == room.RoomId);
        Assert.Equal(RoomStatus.Closed, final.Status);
        Assert.Null(final.HostUserId);
    }

    private static async Task SignInPlayerAsync(HttpClient client)
    {
        var body = new
        {
            email = $"{Guid.NewGuid():N}@example.test",
            displayName = "Investigator",
            password = AccountTestDatabase.Password
        };
        using var register = await AccountRequests.PostAsync(client, "register", body);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        using var login = await AccountRequests.PostAsync(client, "login", body);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    private static async Task<LobbyView> CreateAsync(HttpClient client)
    {
        using var response = await WriteAsync(client, "", new { language = "en" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LobbyView>())!;
    }

    private static Task<HttpResponseMessage> WriteAsync(HttpClient client, string suffix, object body) =>
        AccountRequests.WriteAsync(client, HttpMethod.Post, "/api/v1/rooms" + suffix, body);

    private static async Task AssertError(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(code, error.GetProperty("code").GetString());
    }

    private sealed class FixedCodes : IRoomCodeGenerator
    {
        private int _attempts;
        public string Generate(int length) => Interlocked.Increment(ref _attempts) <= 2 ? "AAAAAA" : "BBBBBB";
    }
}
