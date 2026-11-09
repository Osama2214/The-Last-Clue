namespace LastClue.Application.Security;

public static class InputRules
{
    public const int StatementMaxLength = 1000;
    public const int QuestionMaxLength = 300;
    public const int AnswerMaxLength = 1000;
    public const int DisplayNameMaxLength = 80;

    public static string Trim(string? value) => value?.Trim() ?? string.Empty;
    public static bool IsValidGuid(string? value) => Guid.TryParse(value, out var id) && id != Guid.Empty;
}
