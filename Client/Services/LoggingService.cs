using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace SD.Client.Services
{
    public class LoggingService
    {
        public List<LogEntry> logEntries = new List<LogEntry>();

        public void Log(string name, LogLevel logLevel, string message)
        {
            var logEntry = new LogEntry
            {
                Provider = name,
                Message = message,
                Level = logLevel,
                Timestamp = DateTime.Now
            };
            logEntries.Add(logEntry);
        }
    }
    public class LogEntry
    {
        public string Provider { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
