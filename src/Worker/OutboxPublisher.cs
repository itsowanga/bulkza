using System.Text.Json;
using Application.Interfaces.EventBus;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Worker;

public class OutboxPublisher(ILogger<OutboxPublisher> logger,
                             IServiceScopeFactory scopeFactory,
                             IOptions<OutboxPublisherOptions> options) : BackgroundService
{
    private readonly ILogger<OutboxPublisher> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IOptions<OutboxPublisherOptions> _options = options;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                var batch = await db.Outbox.Where(outbox => outbox.ProcessedAt == null)
                                           .OrderBy(outbox => outbox.OccuredAt)
                                           .Take(_options.Value.BatchSize)
                                           .ToListAsync(stoppingToken);

                if (batch.Count == 0)
                {
                    await Task.Delay(_options.Value.PollInterval, stoppingToken);
                    continue;
                }

                foreach (var message in batch)
                {
                    var evt = Deserialize(message);
                    await bus.PublishAsync(evt, stoppingToken);
                    message.MarkAsProcessed(DateTimeOffset.UtcNow);
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            catch (Exception err)
            {
                _logger.LogError(err, "Outbox publisher iteration failed");
                await Task.Delay(_options.Value.ErrorDelay, stoppingToken);
            }
        }
    }

    private static object Deserialize(OutboxMessage message)
    {
        var type = Type.GetType(message.Type) ?? throw new InvalidOperationException($"Unknown event type: {message.Type}");
        return JsonSerializer.Deserialize(message.Content, type) ?? throw new InvalidOperationException($"Content serialized to null: {message.Type}");
    }


}
