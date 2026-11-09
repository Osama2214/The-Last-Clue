using System.Security.Cryptography;

namespace LastClue.Application.Rooms;

public interface IRoomCodeGenerator
{
    string Generate(int length);
}

public sealed class RoomCodeGenerator : IRoomCodeGenerator
{
    public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const int MaxLength = 12;

    public string Generate(int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MaxLength);

        return string.Create(length, Alphabet, static (characters, alphabet) =>
        {
            for (var index = 0; index < characters.Length; index++)
                characters[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        });
    }

    public static bool IsValid(string code) =>
        code.Length is > 0 and <= MaxLength && code.All(character => Alphabet.Contains(character));
}
