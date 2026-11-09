using System.Text.Json;
using LastClue.Application.Domain;
using LastClue.Application.Rooms;
using LastClue.Application.Settings;
using LastClue.Infrastructure.Persistence;
using LastClue.Web.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LastClue.UI.Tests;

public sealed class RealtimeBrowserTests(BrowserApplication app) : IClassFixture<BrowserApplication>
{
    [SqlBrowserFact]
    public async Task Hub_protects_membership_and_private_groups_and_counts_two_tabs()
    {
        var hostContext = await app.Browser.NewContextAsync();
        var guestContext = await app.Browser.NewContextAsync();
        var outsiderContext = await app.Browser.NewContextAsync();
        try
        {
            var host = await SignInAsync(hostContext, "Host Investigator");
            var guest = await SignInAsync(guestContext, "Guest Investigator");
            var outsider = await SignInAsync(outsiderContext, "Other Investigator");
            var createdJson = await host.EvaluateAsync<string>("async () => (await apiWrite('/api/v1/rooms', 'POST', { language: 'en' })).text()");
            var room = JsonSerializer.Deserialize<LobbyView>(createdJson, JsonSerializerOptions.Web)!;
            Assert.NotEqual(Guid.Empty, room.RoomId);
            var joinedJson = await guest.EvaluateAsync<string>("async code => (await apiWrite('/api/v1/rooms/join', 'POST', { code })).text()", room.Code);
            var joined = JsonSerializer.Deserialize<LobbyView>(joinedJson, JsonSerializerOptions.Web)!;
            Assert.Equal(2, joined.Members.Count);
            var hostId = room.Host!;
            var guestId = joined.Members.Single(member => member.UserId != hostId).UserId;
            var hostTab = await hostContext.NewPageAsync();
            await hostTab.GotoAsync(app.BaseUrl);
            foreach (var page in new[] { host, guest, outsider, hostTab })
                await ConnectAsync(page);

            await Assert.ThrowsAsync<PlaywrightException>(() =>
                outsider.EvaluateAsync("id => hub.joinRoom(id)", room.RoomId.ToString("D")));
            await host.EvaluateAsync("id => hub.joinRoom(id)", room.RoomId.ToString("D"));
            await hostTab.EvaluateAsync("id => hub.joinRoom(id)", room.RoomId.ToString("D"));
            await guest.EvaluateAsync("id => hub.joinRoom(id)", room.RoomId.ToString("D"));
            var presence = app.Services.GetRequiredService<LobbyPresence>();
            Assert.True(presence.IsConnected(room.RoomId, hostId));
            await hostTab.EvaluateAsync("() => hub.stop()");
            Assert.True(presence.IsConnected(room.RoomId, hostId));

            var gameId = await SeedGameAsync(room.RoomId, hostId, guestId);
            await Assert.ThrowsAsync<PlaywrightException>(() =>
                outsider.EvaluateAsync("id => hub.joinGame(id)", gameId.ToString("D")));
            await host.EvaluateAsync("id => hub.joinGame(id)", gameId.ToString("D"));
            await guest.EvaluateAsync("id => hub.joinGame(id)", gameId.ToString("D"));

            var hub = app.Services.GetRequiredService<IHubContext<GameHub>>();
            await hub.Clients.Group(GameHub.PrivateGroup(gameId, hostId)).SendAsync("TestMarker", "host-only");
            await hub.Clients.Group(GameHub.GameGroup(gameId)).SendAsync("TestMarker", "public");
            await host.WaitForFunctionAsync("() => received.includes('public')");
            await guest.WaitForFunctionAsync("() => received.includes('public')");
            Assert.Equal(new[] { "host-only", "public" }, await host.EvaluateAsync<string[]>("() => received"));
            Assert.Equal(new[] { "public" }, await guest.EvaluateAsync<string[]>("() => received"));

            await host.EvaluateAsync("() => hub.stop()");
            await WaitUntilAsync(() => !presence.IsConnected(room.RoomId, hostId));
            await WaitForClosedConnectionAsync(gameId, hostId);
        }
        finally
        {
            await hostContext.CloseAsync();
            await guestContext.CloseAsync();
            await outsiderContext.CloseAsync();
        }
    }

    private async Task<IPage> SignInAsync(IBrowserContext context, string name)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync(app.BaseUrl);
        var email = $"{Guid.NewGuid():N}@example.test";
        var status = await page.EvaluateAsync<int>("""
            async body => {
                const registered = await apiWrite('/api/v1/auth/register', 'POST', body);
                if (!registered.ok) return registered.status;
                return (await apiWrite('/api/v1/auth/login', 'POST', body)).status;
            }
            """, new { email, displayName = name, password = BrowserApplication.Password });
        Assert.Equal(200, status);
        await page.ReloadAsync();
        await Expect(page.GetByText(name, new() { Exact = true })).ToBeVisibleAsync();
        return page;
    }

    private static async Task ConnectAsync(IPage page)
    {
        await page.AddScriptTagAsync(new() { Url = "/lib/signalr/signalr.min.js" });
        await page.AddScriptTagAsync(new() { Url = "/js/game-connection.js" });
        await page.EvaluateAsync("""
            async () => {
                window.hub = createGameConnection();
                window.received = [];
                hub.on('TestMarker', marker => received.push(marker));
                await hub.start();
            }
            """);
    }

    private async Task<Guid> SeedGameAsync(Guid roomId, string host, string guest)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync(host);
        var settingsId = await db.SettingsVersions.Where(item => item.VersionNo == 1)
            .Select(item => item.Id).SingleAsync();
        var game = new Game
        {
            Id = Guid.NewGuid(), RoomId = roomId, Number = 1, Status = GameStatus.Active,
            Round = 1, Phase = Phase.Briefing, SettingsVersionId = settingsId,
            SettingsJson = SettingsDefaults.Json, CreatedAt = DateTime.UtcNow
        };
        game.Players.Add(new GamePlayer { UserId = host, Role = GameRole.Detective, IsActive = true });
        game.Players.Add(new GamePlayer { UserId = guest, Role = GameRole.Criminal, IsActive = true });
        db.Games.Add(game);
        await db.SaveChangesAsync();
        return game.Id;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 40 && !condition(); attempt++)
            await Task.Delay(50);
        Assert.True(condition());
    }

    private async Task WaitForClosedConnectionAsync(Guid gameId, string userId)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        for (var attempt = 0; attempt < 40; attempt++)
        {
            if (await db.PlayerConnections.AnyAsync(row =>
                row.GameId == gameId && row.UserId == userId && row.ClosedAt != null))
                return;
            await Task.Delay(50);
        }
        Assert.Fail("The game connection did not close.");
    }
}
