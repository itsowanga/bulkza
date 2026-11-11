using Application.Interfaces.EventBus;
using Microsoft.Extensions.Logging;

namespace Worker.Tests;

/// <summary>A clock the test moves by hand.</summary>
public sealed class TestTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now = _now.Add(by);
}

/// <summary>Records every publish call and can be told to fail or cancel for chosen messages.</summary>
public sealed class RecordingEventBus : IEventBus
{
    public List<(string Type, string Content)> Published { get; } = [];
    public int Attempts { get; private set; }

    /// <summary>Return an exception to make this publish fail, or null to let it succeed.</summary>
    public Func<string, string, Exception?>? Failure { get; set; }

    public Task PublishAsync(string type, string content, CancellationToken ct = default)
    {
        Attempts++;
        ct.ThrowIfCancellationRequested();

        var failure = Failure?.Invoke(type, content);
        if (failure is not null) throw failure;

        Published.Add((type, content));
        return Task.CompletedTask;
    }
}

/// <summary>Captures log entries so tests can assert on message templates and structured values.</summary>
public sealed class ListLogger<T> : ILogger<T>
{
    public sealed record Entry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Values);

    public List<Entry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var values = (state as IEnumerable<KeyValuePair<string, object?>>)?.ToDictionary(p => p.Key, p => p.Value)
                     ?? new Dictionary<string, object?>();
        Entries.Add(new Entry(logLevel, formatter(state, exception), values));
    }
}
