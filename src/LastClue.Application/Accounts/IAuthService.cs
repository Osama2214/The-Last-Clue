namespace LastClue.Application.Accounts;

/// <summary>Handles account creation and cookie session lifecycle.</summary>
public interface IAuthService
{
    Task<RegistrationResult> RegisterAsync(string email, string displayName, string password);
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync(string userId);
}

public sealed record RegistrationResult(RegistrationStatus Status, IReadOnlyDictionary<string, string>? Fields = null);

public enum RegistrationStatus
{
    Created,
    EmailInUse,
    InvalidAccount
}
