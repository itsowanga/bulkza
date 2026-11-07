using Application.Interfaces.EventBus;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Worker;

public class OutboxPublisher(ILogger<OutboxPublisher> logger,
                             IServiceScopeFactory scopeFactory,
                             IEventBus bus,
                             IOptions<OutboxPublisherOptions> options,
                             TimeProvider timeProvider) : BackgroundService
{
    private readonly ILogger<OutboxPublisher> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IEventBus _bus = bus;
    private readonly OutboxPublisherOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox publisher started. BatchSize={BatchSize} PollInterval={PollInterval} MaxRetryCount={MaxRetryCount}",
                               _options.BatchSize, _options.PollInterval, _options.MaxRetryCount);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var handled = await PublishDueMessagesAsync(stoppingToken);
                if (handled == 0) await Task.Delay(_options.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Outbox publisher iteration failed");
                try
                {
                    await Task.Delay(_options.ErrorDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("Outbox publisher stopped");
    }

    /// <summary>
    /// Runs one polling iteration: reads a batch of due, unprocessed, unparked messages and publishes each one.
    /// Returns the number of messages that reached a final outcome this iteration (published, rescheduled or parked).
    /// Throws <see cref="OperationCanceledException"/> when <paramref name="ct"/> is cancelled.
    /// </summary>
    public async Task<int> PublishDueMessagesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();

        var now = _timeProvider.GetUtcNow();
        var batch = await db.Outbox
            .Where(message => message.ProcessedAt == null
                              && message.ParkedAt == null
                              && (message.NextAttemptOn == null || message.NextAttemptOn <= now))
            .OrderBy(message => message.OccuredAt)
            .Take(_options.BatchSize)
            .ToListAsync(ct);

        var handled = 0;
        foreach (var message in batch)
        {
            ct.ThrowIfCancellationRequested();
            if (await TryPublishAsync(db, message, ct)) handled++;
        }

        return handled;
    }

    private async Task<bool> TryPublishAsync(BulkzaDbContext db, Domain.OutboxMessage message, CancellationToken ct)
    {
        try
        {
            await _bus.PublishAsync(message.Type, message.Content, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutdown: delivery was not confirmed, so the message is left untouched and is not counted as a failure.
            throw;
        }
        catch (Exception err)
        {
            await RecordFailureAsync(db, message, err);
            return true;
        }

        try
        {
            // The event was published, so the processed flag is saved even if shutdown has started.
            // Skipping it would republish the message on the next start. The SQL command timeout bounds the wait.
            message.MarkAsProcessed(_timeProvider.GetUtcNow());
            await db.SaveChangesAsync(CancellationToken.None);
            return true;
        }
        catch (Exception err)
        {
            // Published but not recorded: the message stays due and will be published again (at-least-once delivery).
            // This is not a publish failure, so the retry count is not touched.
            _logger.LogError(err, "Outbox message {MessageId} of type {EventType} was published but could not be marked as processed. It will be published again.",
                             message.Id, message.Type);
            db.Entry(message).State = EntityState.Detached;
            return false;
        }
    }

    private async Task RecordFailureAsync(BulkzaDbContext db, Domain.OutboxMessage message, Exception err)
    {
        var error = string.IsNullOrWhiteSpace(err.Message) ? err.GetType().Name : err.Message;
        var parked = message.RecordFailure(error, _timeProvider.GetUtcNow(), _options.MaxRetryCount,
                                           _options.InitialRetryDelay, _options.MaxRetryDelay);

        // CancellationToken.None: the failure state must be saved even when shutdown starts mid-batch.
        await db.SaveChangesAsync(CancellationToken.None);

        if (parked)
        {
            _logger.LogError(err, "Outbox message {MessageId} of type {EventType} parked after {RetryCount} retries. It will not be retried. Error: {Error}",
                             message.Id, message.Type, message.RetryCount, error);
        }
        else
        {
            _logger.LogWarning(err, "Publishing outbox message {MessageId} of type {EventType} failed. Retry {RetryCount} of {MaxRetryCount} scheduled for {NextAttemptOn}.",
                               message.Id, message.Type, message.RetryCount, _options.MaxRetryCount, message.NextAttemptOn);
        }
    }
}
