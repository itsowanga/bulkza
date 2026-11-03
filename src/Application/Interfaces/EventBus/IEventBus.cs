namespace Application.Interfaces.EventBus;
public interface IEventBus
{
    Task PublishAsync(object integrationEvent, CancellationToken ct = default);
}