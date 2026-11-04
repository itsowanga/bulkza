using System.Collections.Concurrent;
using Application.Interfaces.EventBus;

namespace Worker;
public class InMemoryServiceBus : IEventBus
{
    private readonly ConcurrentQueue<object> _published = new();
    public IReadOnlyList<object> Published => _published.ToList();

    public Task PublishAsync(object integrationEvent, CancellationToken ct = default)
    {
        _published.Enqueue(integrationEvent);
        return Task.CompletedTask;
    }
}