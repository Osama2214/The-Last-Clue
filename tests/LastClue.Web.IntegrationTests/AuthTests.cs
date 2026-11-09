using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LastClue.Infrastructure.Accounts;
using LastClue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace LastClue.Web.IntegrationTests;

public sealed class AuthTests(AccountTestDatabase database) : IClassFixture<AccountTestDatabase>
{
    [SqlServerFact]
    public async Task Register_login_and_logout_revoke_the_original_cookie()
    {
        using var client = database.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.test";
        var body = new { email, displayName = "Test Investigator", password = AccountTestDatabase.Password };
        using var registered = await AccountRequests.PostAsync(client, "register", body);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        using var duplicate = await AccountRequests.PostAsync(client, "register", body);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        using var login = await AccountRequests.PostAsync(client, "login", body);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var header = Assert.Single(login.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
        Assert.Contains("secure", header);
        Assert.Contains("httponly", header);
        Assert.Contains("samesite=lax", header);

        using var profile = await client.GetAsync("/api/v1/profiles/me");
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        await using var scope = database.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = Assert.IsType<AppUser>(await users.FindByEmailAsync(email));
        Assert.True(await users.IsInRoleAsync(user, AccountIdentity.Player));
        Assert.True(Guid.TryParseExact(user.AvatarSeed, "N", out _));

        using var logout = await AccountRequests.PostAsync(client, "logout");
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        using var loggedOut = await client.GetAsync("/api/v1/profiles/me");
        Assert.Equal(HttpStatusCode.Unauthorized, loggedOut.StatusCode);
        using var replay = database.CreateClient(handleCookies: false);
        replay.DefaultRequestHeaders.Add("Cookie", header.Split(';', 2)[0]);
        using var replayed = await replay.GetAsync("/api/v1/profiles/me");
        Assert.Equal(HttpStatusCode.Unauthorized, replayed.StatusCode);
    }

    [SqlServerFact]
    public async Task Missing_wrong_and_blocked_accounts_get_the_same_login_error()
    {
        using var client = database.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.test";
        var password = AccountTestDatabase.Password;
        using var registered = await AccountRequests.PostAsync(client, "register",
            new { email, displayName = "Blocked Investigator", password });
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        var missing = await LoginError(client, "missing@example.test", password);
        var wrong = await LoginError(client, email, "Incorrect8");

        await using var scope = database.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = Assert.IsType<AppUser>(await users.FindByEmailAsync(email));
        user.IsBlocked = true;
        Assert.True((await users.UpdateAsync(user)).Succeeded);
        var blocked = await LoginError(client, email, password);
        Assert.Equal(("INVALID_CREDENTIALS", "Invalid email or password."), missing);
        Assert.Equal(missing, wrong);
        Assert.Equal(missing, blocked);
    }

    [SqlServerFact]
    public async Task Admin_seed_is_idempotent_and_password_policy_rejects_weak_passwords()
    {
        await using var scope = database.Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IdentitySeeder>()
            .SeedAsync(AccountTestDatabase.AdminEmail, AccountTestDatabase.Password);
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = Assert.Single(await users.GetUsersInRoleAsync(AccountIdentity.Admin));
        Assert.Equal(AccountTestDatabase.AdminEmail, admin.Email);
        using var client = database.CreateClient();
        using var invalid = await AccountRequests.PostAsync(client, "register",
            new { email = "weak@example.test", displayName = "Weak Password", password = "longbutnodigits" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Null(await users.FindByEmailAsync("weak@example.test"));
    }

    [Fact]
    public async Task Expired_session_returns_json_without_redirect()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false
        });
        var scheme = IdentityConstants.ApplicationScheme;
        var options = factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(scheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "expired-user")], scheme));
        var ticket = new AuthenticationTicket(principal, new AuthenticationProperties
        {
            IssuedUtc = DateTimeOffset.UtcNow.AddHours(-9), ExpiresUtc = DateTimeOffset.UtcNow.AddHours(-1)
        }, scheme);
        client.DefaultRequestHeaders.Add("Cookie", $"{options.Cookie.Name}={options.TicketDataFormat.Protect(ticket)}");

        using var response = await client.GetAsync("/api/v1/profiles/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Password_validator_requires_eight_characters_and_a_digit()
    {
        using var factory = new WebApplicationFactory<Program>();
        await using var scope = factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var validator = new PasswordValidator<AppUser>();
        var user = new AppUser();
        Assert.False((await validator.ValidateAsync(users, user, "Short8")).Succeeded);
        Assert.False((await validator.ValidateAsync(users, user, "allletters")).Succeeded);
        Assert.True((await validator.ValidateAsync(users, user, "correct8")).Succeeded);
    }

    [SqlServerFact]
    public async Task Display_name_is_limited_stored_literally_and_html_encoded()
    {
        using var client = database.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.test";
        var password = AccountTestDatabase.Password;
        using var tooLong = await AccountRequests.PostAsync(client, "register",
            new { email, displayName = new string('a', 81), password });
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        var error = await tooLong.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(error.GetProperty("fields").TryGetProperty("displayName", out _));

        const string displayName = "<script>alert(1)</script> '; DROP TABLE x;--";
        using var registered = await AccountRequests.PostAsync(client, "register", new { email, displayName, password });
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        using var login = await AccountRequests.PostAsync(client, "login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        await using var scope = database.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = Assert.IsType<AppUser>(await users.FindByEmailAsync(email));
        Assert.Equal(displayName, user.DisplayName);
        using var home = await client.GetAsync("/");
        var html = await home.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<script>alert(1)</script>", html);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
    }

    private static async Task<(string? Code, string? Message)> LoginError(
        HttpClient client, string email, string password)
    {
        using var response = await AccountRequests.PostAsync(client, "login", new { email, password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (body.GetProperty("code").GetString(), body.GetProperty("message").GetString());
    }
}
