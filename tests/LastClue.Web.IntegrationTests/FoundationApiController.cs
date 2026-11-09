using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LastClue.Application.Exceptions;
using LastClue.Web.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LastClue.Web.IntegrationTests;

// Registered only by ApiTestFactory; no test routes are published with the application.
public sealed class FoundationApiController : ApiControllerBase
{
    [HttpGet("errors/{kind}")]
    public IActionResult Error(string kind) => throw ErrorFor(kind);

    [HttpGet("status/{status:int}")]
    public IActionResult EmptyStatus(int status) => StatusCode(status);

    [HttpGet("custom-error/{status:int}")]
    public IActionResult CustomError(int status) => status == 400
        ? new ContentResult { StatusCode = 400, Content = "private-result-sentinel", ContentType = "text/html" }
        : StatusCode(status, new { detail = "private-result-sentinel" });

    [HttpPost("validate")]
    public IActionResult Validate(Input input) => Ok(new { accepted = true });

    [Authorize]
    [HttpGet("secured")]
    public IActionResult Secured() => Ok();

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult Admin() => Ok();

    [HttpPost("session")]
    public async Task<IActionResult> Session()
    {
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "ec27d53b-f67a-447e-9e3c-063ca54cf56c"),
            new Claim(ClaimTypes.Email, "private-email-sentinel@example.test"),
            new Claim(ClaimTypes.Name, "private-name-sentinel"),
            new Claim(ClaimTypes.Role, "Player")], IdentityConstants.ApplicationScheme);
        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));
        return NoContent();
    }

    [HttpPost("/api/v1/games/{gameId}/diagnostics-test")]
    [HttpPost("/api/v1/admin/cases/{gameId}/diagnostics-test")]
    [HttpPost("/api/v1/auth/{gameId}/diagnostics-test")]
    public async Task<IActionResult> SecretFailure(string gameId)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var failure = new InvalidOperationException(body, new Exception("inner-private-sentinel"));
        failure.Data["private"] = "data-private-sentinel";
        throw failure;
    }

    internal static Exception ErrorFor(string kind, string detail = "private-error-sentinel") => kind switch
    {
        "validation" => new LastClue.Application.Exceptions.ValidationException(detail),
        "forbidden" => new ForbiddenException(detail),
        "not-found" => new NotFoundException(detail),
        "phase" => new ConflictException(DomainErrorCode.PhaseChanged, 42, detail),
        "room-full" => new ConflictException(DomainErrorCode.RoomFull, detail: detail),
        "already-in-room" => new ConflictException(DomainErrorCode.AlreadyInRoom, detail: detail),
        "concurrency" => new ConcurrencyConflictException(),
        "duplicate" => new DuplicateException(),
        "replay" => new ConflictException(DomainErrorCode.ReplayMismatch, detail: detail),
        "rate-limit" => new RateLimitedException(30, detail),
        _ => new InvalidOperationException(detail)
    };

    public sealed class Input
    {
        [Required, StringLength(10)]
        public string? Text { get; set; }
    }
}
