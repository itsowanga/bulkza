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

                var batch = await db.Outbox.Where(outbox => outbox.ProcessedAt == null && outbox.NextAttemptOn <= DateTimeOffset.UtcNow)
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
                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        var evt = Deserialize(message);
                        await bus.PublishAsync(evt, stoppingToken);
                        message.MarkAsProcessed(DateTimeOffset.UtcNow);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }         
                    catch (Exception err)
                    {
                        _logger.LogError(err, "Publish failed for outbox message {MessageId}", message.Id);
                        using var failScope = _scopeFactory.CreateScope();
                        var failDb = failScope.ServiceProvider.GetRequiredService<BulkzaDbContext>();

                        var failedMessage = await failDb.Outbox.FindAsync([message.Id], CancellationToken.None);

                        if (failedMessage is not null)
                        {
                            failedMessage.RecordFailure(err.Message);
                            await failDb.SaveChangesAsync(CancellationToken.None);
                        }

                        await Task.Delay(_options.Value.ErrorDelay, CancellationToken.None);
                    }      
                }
            }
            
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            catch (Exception err)
            {
                _logger.LogError(err, "Outbox publisher iteration failed");   
                await Task.Delay(_options.Value.ErrorDelay, CancellationToken.None);
            }
        }
    }

    private static object Deserialize(OutboxMessage message)
    {
        var type = Type.GetType(message.Type) ?? throw new InvalidOperationException($"Unknown event type: {message.Type}");
        return JsonSerializer.Deserialize(message.Content, type) ?? throw new InvalidOperationException($"Content serialized to null: {message.Type}");
    }


}
