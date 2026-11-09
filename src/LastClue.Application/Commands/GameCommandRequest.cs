using System.Security.Cryptography;
using System.Text.Json;
using LastClue.Application.Exceptions;

namespace LastClue.Application.Commands;

public sealed class GameCommandRequest
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private GameCommandRequest(string userId, Guid requestId, Guid? gameId, long? expectedVersion, string payloadHash)
    {
        UserId = userId;
        RequestId = requestId;
        GameId = gameId;
        ExpectedVersion = expectedVersion;
        PayloadHash = payloadHash;
    }

    public string UserId { get; }
    public Guid RequestId { get; }
    public Guid? GameId { get; }
    public long? ExpectedVersion { get; }
    public string PayloadHash { get; }

    public static GameCommandRequest Create<T>(
        string userId, Guid requestId, string operation, Guid? gameId, long? expectedVersion, T payload)
    {
        if (string.IsNullOrWhiteSpace(userId) || requestId == Guid.Empty || string.IsNullOrWhiteSpace(operation)
            || gameId == Guid.Empty || expectedVersion < 0 || gameId.HasValue != expectedVersion.HasValue)
            throw new ValidationException("The game command identity or expected version is invalid.");

        var envelope = JsonSerializer.SerializeToElement(new { operation, gameId, expectedVersion, payload }, JsonOptions);
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
            WriteCanonical(writer, envelope);

        return new GameCommandRequest(userId, requestId, gameId, expectedVersion,
            Convert.ToHexString(SHA256.HashData(buffer.GetBuffer().AsSpan(0, checked((int)buffer.Length)))));
    }

    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                string? previousName = null;
                foreach (var property in value.EnumerateObject().OrderBy(item => item.Name, StringComparer.Ordinal))
                {
                    if (property.Name == previousName)
                        throw new ValidationException("Duplicate JSON properties are not supported.");
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(writer, property.Value);
                    previousName = property.Name;
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray())
                    WriteCanonical(writer, item);
                writer.WriteEndArray();
                break;
            default:
                value.WriteTo(writer);
                break;
        }
    }
}
