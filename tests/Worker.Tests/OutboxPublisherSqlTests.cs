using Domain;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Worker.Tests;

/// <summary>
/// Runs the real publisher against a real SQL Server container (no EF InMemory).
/// Each test drives <see cref="OutboxPublisher.PublishDueMessagesAsync"/> directly with a manual clock,
/// so nothing depends on timers or sleeping.
/// </summary>
[Collection("outbox-sql")]
public class OutboxPublisherSqlTests(OutboxSqlFixture fixture) : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private readonly OutboxSqlFixture _fixture = fixture;
    private readonly TestTimeProvider _time = new(Start);
    private readonly RecordingEventBus _bus = new();

    public Task InitializeAsync() => _fixture.ClearOutboxAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private OutboxPublisher CreatePublisher(OutboxPublisherOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddDbContext<BulkzaDbContext>(o => o.UseSqlServer(_fixture.ConnectionString));
        var provider = services.BuildServiceProvider();

        return new OutboxPublisher(NullLogger<OutboxPublisher>.Instance,
                                   provider.GetRequiredService<IServiceScopeFactory>(),
                                   _bus,
                                   Options.Create(options ?? new OutboxPublisherOptions
                                   {
                                       MaxRetryCount = 3,
                                       InitialRetryDelay = TimeSpan.FromSeconds(10),
                                       MaxRetryDelay = TimeSpan.FromSeconds(25),
                                   }),
                                   _time);
    }

    private async Task SeedAsync(params OutboxMessage[] messages)
    {
        await using var db = _fixture.CreateDbContext();
        db.Outbox.AddRange(messages);
        await db.SaveChangesAsync();
    }

    private async Task<OutboxMessage> LoadAsync(Guid id)
    {
        await using var db = _fixture.CreateDbContext();
        return await db.Outbox.AsNoTracking().SingleAsync(m => m.Id == id);
    }

    private static OutboxMessage Message(string type = "Domain.Events.PoolLocked, Domain", string content = """{"n":1}""", int minutesAgo = 0) =>
        OutboxMessage.Create(type, content, Start.AddMinutes(-minutesAgo), null);

    // 1. Worker identifies and retrieves due, unprocessed messages.
    [Fact]
    public async Task PublishDueMessages_PublishesOnlyDueUnprocessedUnparkedMessages_OldestFirst()
    {
        var older = Message(content: """{"n":"older"}""", minutesAgo: 30);
        var newer = Message(content: """{"n":"newer"}""", minutesAgo: 10);
        var dueRetry = OutboxMessage.From(Guid.NewGuid(), "t", """{"n":"retry"}""", Start.AddMinutes(-20), null, "old error", 1, Start.AddSeconds(-1));
        var processed = OutboxMessage.Create("t", """{"n":"processed"}""", Start.AddMinutes(-40), Start.AddMinutes(-39));
        var parked = OutboxMessage.From(Guid.NewGuid(), "t", """{"n":"parked"}""", Start.AddMinutes(-50), null, "gave up", 3, Start.AddMinutes(-45), Start.AddMinutes(-44));
        await SeedAsync(newer, processed, parked, dueRetry, older);

        var handled = await CreatePublisher().PublishDueMessagesAsync(CancellationToken.None);

        handled.Should().Be(3);
        _bus.Published.Select(p => p.Content).Should().Equal("""{"n":"older"}""", """{"n":"retry"}""", """{"n":"newer"}""");
    }

    // 2. Future-scheduled messages are not published.
    [Fact]
    public async Task PublishDueMessages_FutureScheduledMessage_IsNotPublishedUntilItIsDue()
    {
        var scheduled = OutboxMessage.From(Guid.NewGuid(), "t", "{}", Start.AddMinutes(-5), null, "e", 1, Start.AddMinutes(10));
        await SeedAsync(scheduled);
        var publisher = CreatePublisher();

        (await publisher.PublishDueMessagesAsync(CancellationToken.None)).Should().Be(0);
        _bus.Published.Should().BeEmpty();
        (await LoadAsync(scheduled.Id)).ProcessedAt.Should().BeNull();

        _time.Advance(TimeSpan.FromMinutes(10));
        (await publisher.PublishDueMessagesAsync(CancellationToken.None)).Should().Be(1);
        _bus.Published.Should().ContainSingle();
    }

    // 3. Successfully published messages receive ProcessedAt (and it is persisted).
    [Fact]
    public async Task PublishDueMessages_Success_SetsProcessedAtAndPersistsIt()
    {
        var message = Message();
        await SeedAsync(message);
        _time.Advance(TimeSpan.FromMinutes(3));

        await CreatePublisher().PublishDueMessagesAsync(CancellationToken.None);

        var saved = await LoadAsync(message.Id);
        saved.ProcessedAt.Should().Be(_time.GetUtcNow());
        saved.RetryCount.Should().Be(0);
        saved.Error.Should().BeNull();
        saved.IsParked.Should().BeFalse();
    }

    [Fact]
    public async Task PublishDueMessages_ProcessedMessage_IsNotPublishedAgainOnTheNextPoll()
    {
        await SeedAsync(Message());
        var publisher = CreatePublisher();

        await publisher.PublishDueMessagesAsync(CancellationToken.None);
        var second = await publisher.PublishDueMessagesAsync(CancellationToken.None);

        second.Should().Be(0);
        _bus.Published.Should().ContainSingle();
    }

    // 4. Raw JSON content and event type are passed unchanged. Nothing is deserialised.
    [Fact]
    public async Task PublishDueMessages_PassesTypeAndRawJsonUnchanged_EvenForTypesThatDoNotExist()
    {
        const string type = "Some.Unknown.Event, NoSuchAssembly, Version=9.9.9.9";
        const string content = "{ \"weird\" :  [1,2,3 ],\n  \"text\": \"caf\u00e9 \\u00e9\", \"nested\":{\"Value\":\"00000000-0000-0000-0000-000000000000\"} }";
        await SeedAsync(OutboxMessage.Create(type, content, Start, null));

        await CreatePublisher().PublishDueMessagesAsync(CancellationToken.None);

        _bus.Published.Should().ContainSingle().Which.Should().Be((type, content));
    }

    // 5. Publishing failures call RecordFailure. 6. Retry count increments.
    [Fact]
    public async Task PublishDueMessages_Failure_RecordsErrorAndSchedulesRetry()
    {
        var message = Message();
        await SeedAsync(message);
        _bus.Failure = (_, _) => new InvalidOperationException("broker is down");

        var handled = await CreatePublisher().PublishDueMessagesAsync(CancellationToken.None);

        handled.Should().Be(1);
        var saved = await LoadAsync(message.Id);
        saved.ProcessedAt.Should().BeNull();
        saved.RetryCount.Should().Be(1);
        saved.Error.Should().Be("broker is down");
        saved.NextAttemptOn.Should().Be(Start.AddSeconds(10));
        saved.IsParked.Should().BeFalse();
    }

    [Fact]
    public async Task PublishDueMessages_FailureWithEmptyMessage_StillRecordsAUsableError()
    {
        var message = Message();
        await SeedAsync(message);
        _bus.Failure = (_, _) => new TimeoutException("");

        await CreatePublisher().PublishDueMessagesAsync(CancellationToken.None);

        (await LoadAsync(message.Id)).Error.Should().Be(nameof(TimeoutException));
    }

    // 6, 7, 8. Retry count, exponential backoff with cap, parking at the limit.
    [Fact]
    public async Task PublishDueMessages_RepeatedFailures_BackOffExponentiallyWithCapThenPark()
    {
        // MaxRetryCount 3 = 1 first attempt + 3 retries = 4 attempts. Delays: 10s, 20s, then 40s capped to 25s.
        var message = Message();
        await SeedAsync(message);
        _bus.Failure = (_, _) => new InvalidOperationException("still down");
        var publisher = CreatePublisher();

        await publisher.PublishDueMessagesAsync(CancellationToken.None);            // attempt 1
        var afterFirst = await LoadAsync(message.Id);
        afterFirst.RetryCount.Should().Be(1);
        afterFirst.NextAttemptOn.Should().Be(_time.GetUtcNow().AddSeconds(10));

        _time.Advance(TimeSpan.FromSeconds(9));                                      // not due yet
        (await publisher.PublishDueMessagesAsync(CancellationToken.None)).Should().Be(0);
        _bus.Attempts.Should().Be(1);

        _time.Advance(TimeSpan.FromSeconds(1));
        await publisher.PublishDueMessagesAsync(CancellationToken.None);            // attempt 2
        var afterSecond = await LoadAsync(message.Id);
        afterSecond.RetryCount.Should().Be(2);
        afterSecond.NextAttemptOn.Should().Be(_time.GetUtcNow().AddSeconds(20));

        _time.Advance(TimeSpan.FromSeconds(20));
        await publisher.PublishDueMessagesAsync(CancellationToken.None);            // attempt 3
        var afterThird = await LoadAsync(message.Id);
        afterThird.RetryCount.Should().Be(3);
        afterThird.NextAttemptOn.Should().Be(_time.GetUtcNow().AddSeconds(25), "40s is capped to MaxRetryDelay");

        _time.Advance(TimeSpan.FromSeconds(25));
        await publisher.PublishDueMessagesAsync(CancellationToken.None);            // attempt 4: no retries left
        var parked = await LoadAsync(message.Id);
        parked.IsParked.Should().BeTrue();
        parked.ParkedAt.Should().Be(_time.GetUtcNow());
        parked.RetryCount.Should().Be(3);
        parked.Error.Should().Be("still down");
        parked.ProcessedAt.Should().BeNull();
        _bus.Attempts.Should().Be(4);

        _time.Advance(TimeSpan.FromDays(7));                                         // parked messages are never retried
        (await publisher.PublishDueMessagesAsync(CancellationToken.None)).Should().Be(0);
        _bus.Attempts.Should().Be(4);
    }

    [Fact]
    public async Task PublishDueMessages_ZeroMaxRetryCount_ParksOnTheFirstFailure()
    {
        var message = Message();
        await SeedAsync(message);
        _bus.Failure = (_, _) => new InvalidOperationException("nope");

        await CreatePublisher(new OutboxPublisherOptions { MaxRetryCount = 0 }).PublishDueMessagesAsync(CancellationToken.None);

        var saved = await LoadAsync(message.Id);
        saved.IsParked.Should().BeTrue();
        saved.RetryCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishDueMessages_MessageThatSucceedsOnARetry_IsProcessedAndNotParked()
    {
        var message = Message();
        await SeedAsync(message);
        var publisher = CreatePublisher();

        _bus.Failure = (_, _) => new InvalidOperationException("blip");
        await publisher.PublishDueMessagesAsync(CancellationToken.None);
        _bus.Failure = null;
        _time.Advance(TimeSpan.FromSeconds(10));
        await publisher.PublishDueMessagesAsync(CancellationToken.None);

        var saved = await LoadAsync(message.Id);
        saved.ProcessedAt.Should().Be(_time.GetUtcNow());
        saved.RetryCount.Should().Be(1);
        saved.IsParked.Should().BeFalse();
    }

    // 9. A failure in one message does not prevent processing the remaining batch.
    [Fact]
    public async Task PublishDueMessages_OneFailingMessage_DoesNotStopTheRestOfTheBatch()
    {
        var first = Message(content: """{"n":1}""", minutesAgo: 3);
        var failing = Message(content: """{"n":"bad"}""", minutesAgo: 2);
        var last = Message(content: """{"n":3}""", minutesAgo: 1);
        await SeedAsync(first, failing, last);
        _bus.Failure = (_, content) => content.Contains("bad") ? new InvalidOperationException("poison") : null;

        var handled = await CreatePublisher().PublishDueMessagesAsync(CancellationToken.None);

        handled.Should().Be(3);
        _bus.Published.Select(p => p.Content).Should().Equal("""{"n":1}""", """{"n":3}""");
        (await LoadAsync(first.Id)).ProcessedAt.Should().NotBeNull();
        (await LoadAsync(last.Id)).ProcessedAt.Should().NotBeNull();
        var failed = await LoadAsync(failing.Id);
        failed.ProcessedAt.Should().BeNull();
        failed.RetryCount.Should().Be(1);
    }

    [Fact]
    public async Task PublishDueMessages_RespectsBatchSize()
    {
        await SeedAsync(Message(minutesAgo: 4), Message(minutesAgo: 3), Message(minutesAgo: 2), Message(minutesAgo: 1));
        var publisher = CreatePublisher(new OutboxPublisherOptions { BatchSize = 3 });

        (await publisher.PublishDueMessagesAsync(CancellationToken.None)).Should().Be(3);
        (await publisher.PublishDueMessagesAsync(CancellationToken.None)).Should().Be(1);
        _bus.Published.Should().HaveCount(4);
    }

    // 12. Graceful cancellation does not mark a message as successfully processed or as failed.
    [Fact]
    public async Task PublishDueMessages_CancelledWhilePublishing_LeavesTheMessageUntouched()
    {
        var interrupted = Message(minutesAgo: 2);
        var waiting = Message(minutesAgo: 1);
        await SeedAsync(interrupted, waiting);
        using var cts = new CancellationTokenSource();
        _bus.Failure = (_, _) =>
        {
            cts.Cancel();
            return new OperationCanceledException(cts.Token);
        };

        var act = () => CreatePublisher().PublishDueMessagesAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        foreach (var id in new[] { interrupted.Id, waiting.Id })
        {
            var saved = await LoadAsync(id);
            saved.ProcessedAt.Should().BeNull();
            saved.RetryCount.Should().Be(0);
            saved.Error.Should().BeNull();
            saved.NextAttemptOn.Should().BeNull();
            saved.IsParked.Should().BeFalse();
        }
        _bus.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishDueMessages_AlreadyCancelled_ThrowsAndPublishesNothing()
    {
        await SeedAsync(Message());
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => CreatePublisher().PublishDueMessagesAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        _bus.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishDueMessages_CancelledMidBatch_StopsBeforeTheNextMessage()
    {
        var first = Message(minutesAgo: 2);
        var second = Message(minutesAgo: 1);
        await SeedAsync(first, second);
        using var cts = new CancellationTokenSource();
        var bus = new CancelAfterFirstPublishBus(cts);
        var publisher = new OutboxPublisher(NullLogger<OutboxPublisher>.Instance, BuildScopeFactory(), bus,
                                            Options.Create(new OutboxPublisherOptions()), _time);

        var act = () => publisher.PublishDueMessagesAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        (await LoadAsync(first.Id)).ProcessedAt.Should().NotBeNull("it was published before shutdown, so it is recorded");
        (await LoadAsync(second.Id)).ProcessedAt.Should().BeNull();
        bus.Published.Should().Be(1);
    }

    // The hosted loop starts, publishes and shuts down promptly.
    [Fact]
    public async Task HostedLoop_PublishesPendingMessages_AndStopsGracefully()
    {
        var message = Message();
        await SeedAsync(message);
        var publisher = CreatePublisher(new OutboxPublisherOptions { PollInterval = TimeSpan.FromMilliseconds(50) });

        await publisher.StartAsync(CancellationToken.None);
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline && (await LoadAsync(message.Id)).ProcessedAt is null) await Task.Delay(50);

        using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await publisher.StopAsync(stopTimeout.Token);

        (await LoadAsync(message.Id)).ProcessedAt.Should().NotBeNull();
        _bus.Published.Should().ContainSingle();
        publisher.ExecuteTask!.IsCompletedSuccessfully.Should().BeTrue();
    }

    private IServiceScopeFactory BuildScopeFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<BulkzaDbContext>(o => o.UseSqlServer(_fixture.ConnectionString));
        return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private sealed class CancelAfterFirstPublishBus(CancellationTokenSource cts) : Application.Interfaces.EventBus.IEventBus
    {
        public int Published { get; private set; }

        public Task PublishAsync(string type, string content, CancellationToken ct = default)
        {
            Published++;
            cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
