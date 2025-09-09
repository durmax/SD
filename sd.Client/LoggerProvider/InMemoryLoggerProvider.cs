namespace sd.Client.LoggerProvider;

using Microsoft.Extensions.Logging;
using System;
using System.Text;

public sealed class InMemoryLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();
    private readonly InMemoryLogStore _store;

    public InMemoryLoggerProvider(InMemoryLogStore store) => _store = store;

    public ILogger CreateLogger(string categoryName) => new InMemoryLogger(categoryName, _store, () => _scopes);
    public void Dispose() { }
    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;

    private sealed class InMemoryLogger(string category, InMemoryLogStore store, Func<IExternalScopeProvider> scopes) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => scopes().Push(state)!;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string? scopeText = null;
            scopes().ForEachScope<object>((scope, _) =>
            {
                var sb = new StringBuilder(scopeText);
                if (sb.Length > 0) sb.Append(" => ");
                sb.Append(scope);
                scopeText = sb.ToString();
            }, null);

            var entry = new LogEntry
            {
                TimestampUtc = DateTime.UtcNow,
                Category = category,
                Level = logLevel,
                EventId = eventId,
                Message = formatter(state, exception),
                Exception = exception,
                Scope = scopeText
            };
            store.Add(entry);
        }
    }
}

