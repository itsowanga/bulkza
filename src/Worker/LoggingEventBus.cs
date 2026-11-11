using Application.Interfaces.EventBus;

namespace Worker;

/// <summary>
/// Development fallback that writes each event to the log. It is not a message broker:
/// nothing is stored, delivered, or replayable, and a "successful" publish only means the line was logged.
/// Replace it with a real broker implementation (for example Azure Service Bus) before relying on delivery.
/// </summary>
public class LoggingEventBus(ILogger<LoggingEventBus> logger) : IEventBus
{
    private readonly ILogger<LoggingEventBus> _logger = logger;

    public Task PublishAsync(string type, string content, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        _logger.LogInformation("Outbox event {EventType} published to logging bus with content {EventContent}", type, content);
        return Task.CompletedTask;
    }
}
