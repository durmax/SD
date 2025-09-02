using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sd.Client.LoggerProvider;

public sealed class InMemoryLogStore
{
    private readonly int _capacity;
    private readonly LinkedList<LogEntry> _entries = new();
    private readonly object _gate = new();

    public event Action? Changed;

    public InMemoryLogStore(int capacity = 500)
        => _capacity = capacity;

    public IReadOnlyCollection<LogEntry> Snapshot()
    {
        lock (_gate)
            return _entries.ToList().AsReadOnly();
    }

    internal void Add(LogEntry entry)
    {
        lock (_gate)
        {
            _entries.AddLast(entry);
            if (_entries.Count > _capacity)
                _entries.RemoveFirst();
        }
        Changed?.Invoke();
    }
}

