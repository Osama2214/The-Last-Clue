using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LastClue.Web.Api;
using LastClue.Application.Accounts;
using LastClue.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastClue.Web.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AuthController(IAuthService auth) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegistrationRequest request)
    {
        var result = await auth.RegisterAsync(request.Email, request.DisplayName, request.Password);
        return result.Status switch
        {
            RegistrationStatus.Created => StatusCode(StatusCodes.Status201Created),
            RegistrationStatus.EmailInUse => AccountError(409, "EMAIL_IN_USE", "That email is already registered.",
                result.Fields),
            _ => AccountError(400, "INVALID_INPUT", "Check the registration fields.", result.Fields)
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request) =>
        await auth.LoginAsync(request.Email, request.Password) ? Ok()
            : AccountError(401, "INVALID_CREDENTIALS", "Invalid email or password.");

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await auth.LogoutAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return NoContent();
    }

    private ObjectResult AccountError(int status, string code, string message,
        IReadOnlyDictionary<string, string>? fields = null)
    {
        var error = new ApiError(code, message, ApiRequest.CorrelationId(HttpContext), Fields: fields);
        return StatusCode(status, error);
    }
}

public sealed record RegistrationRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(InputRules.DisplayNameMaxLength)] string DisplayName,
    [Required, StringLength(128, MinimumLength = 8)] string Password);

public sealed record LoginRequest(
    [Required, StringLength(256)] string Email,
    [Required, StringLength(128)] string Password);
