using System.Text.Json.Serialization;

namespace LastClue.Web.Api;

public sealed record ApiError(string Code, string Message, string CorrelationId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? CurrentStateVersion = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, string>? Fields = null);
