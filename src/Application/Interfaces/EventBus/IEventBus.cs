namespace Application.Interfaces.EventBus;

/// <summary>
/// Publishes a stored outbox event. The event type name and raw JSON content are passed through unchanged;
/// the bus never deserialises the payload.
/// </summary>
public interface IEventBus
{
    Task PublishAsync(string type, string content, CancellationToken ct = default);
}
