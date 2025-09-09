using Microsoft.Extensions.Logging;
using System;

namespace sd.Client.LoggerProvider;
public sealed class LogEntry
{
    public DateTime TimestampUtc { get; init; }
    public string Category { get; init; } = default!;
    public LogLevel Level { get; init; }
    public EventId EventId { get; init; }
    public string Message { get; init; } = default!;
    public Exception? Exception { get; init; }
    public string? Scope { get; init; }
}
