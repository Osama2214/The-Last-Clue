using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace LastClue.Web.IntegrationTests;

internal sealed class DiagnosticLogCapture : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();
    public ConcurrentQueue<Entry> Entries { get; } = new();
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;
    public ILogger CreateLogger(string categoryName) => new CaptureLogger(this, categoryName);
    public void Dispose() { }

    internal sealed record Entry(string Category, string? Event, string Message,
        Dictionary<string, object?> Fields, string Scopes, Exception? Exception);

    private sealed class CaptureLogger(DiagnosticLogCapture capture, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => capture._scopes.Push(state);
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var scopes = new List<string>();
            capture._scopes.ForEachScope((scope, list) =>
            {
                list.Add(scope?.ToString() ?? string.Empty);
                if (scope is IEnumerable<KeyValuePair<string, object?>> fields)
                    list.AddRange(fields.Select(field => field.Key + "=" + field.Value));
            }, scopes);
            var values = state is IEnumerable<KeyValuePair<string, object?>> pairs
                ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value) : new Dictionary<string, object?>();
            capture.Entries.Enqueue(new(category, eventId.Name, formatter(state, exception), values,
                string.Join("\n", scopes), exception));
        }
    }
}
