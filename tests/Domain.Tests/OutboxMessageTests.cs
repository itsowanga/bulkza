using FluentAssertions;

namespace Domain.Tests;

public class OutboxMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Initial = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Max = TimeSpan.FromSeconds(60);

    private static OutboxMessage NewMessage() => OutboxMessage.Create("Domain.Events.PoolLocked", """{"PoolId":{"Value":"x"}}""", Now, null);

    [Fact]
    public void RecordFailure_FirstFailure_SchedulesRetryOneAtInitialDelay()
    {
        var message = NewMessage();

        var parked = message.RecordFailure("boom", Now, 5, Initial, Max);

        parked.Should().BeFalse();
        message.RetryCount.Should().Be(1);
        message.NextAttemptOn.Should().Be(Now.AddSeconds(10));
        message.Error.Should().Be("boom");
        message.IsParked.Should().BeFalse();
    }

    [Fact]
    public void RecordFailure_RepeatedFailures_IncrementRetryCountAndDoubleTheDelay()
    {
        var message = NewMessage();

        message.RecordFailure("e", Now, 10, Initial, TimeSpan.FromHours(1));
        message.NextAttemptOn.Should().Be(Now.AddSeconds(10));
        message.RecordFailure("e", Now, 10, Initial, TimeSpan.FromHours(1));
        message.NextAttemptOn.Should().Be(Now.AddSeconds(20));
        message.RecordFailure("e", Now, 10, Initial, TimeSpan.FromHours(1));
        message.NextAttemptOn.Should().Be(Now.AddSeconds(40));
        message.RecordFailure("e", Now, 10, Initial, TimeSpan.FromHours(1));
        message.NextAttemptOn.Should().Be(Now.AddSeconds(80));

        message.RetryCount.Should().Be(4);
    }

    [Fact]
    public void RecordFailure_DelayIsCappedAtMaxRetryDelay()
    {
        var message = NewMessage();

        // 10s, 20s, 40s, then 80s capped to 60s, then stays at 60s.
        for (var i = 0; i < 3; i++) message.RecordFailure("e", Now, 10, Initial, Max);
        message.RecordFailure("e", Now, 10, Initial, Max);
        message.NextAttemptOn.Should().Be(Now.AddSeconds(60));
        message.RecordFailure("e", Now, 10, Initial, Max);
        message.NextAttemptOn.Should().Be(Now.AddSeconds(60));
    }

    [Fact]
    public void RecordFailure_NeverOverflowsForLargeRetryCounts()
    {
        var message = OutboxMessage.From(Guid.NewGuid(), "t", "c", Now, null, null, retry: 500, nextAttempt: null);

        var parked = message.RecordFailure("e", Now, 1000, Initial, Max);

        parked.Should().BeFalse();
        message.NextAttemptOn.Should().Be(Now.AddSeconds(60));
    }

    [Fact]
    public void RecordFailure_MaxRetryCountIsRetriesAfterTheFirstAttempt_ParksOnTheFollowingFailure()
    {
        var message = NewMessage();
        const int maxRetryCount = 3;

        // Failures 1 to 3 each schedule a retry (3 retries). Failure 4 has no retries left and parks.
        message.RecordFailure("e", Now, maxRetryCount, Initial, Max).Should().BeFalse();
        message.RecordFailure("e", Now, maxRetryCount, Initial, Max).Should().BeFalse();
        message.RecordFailure("e", Now, maxRetryCount, Initial, Max).Should().BeFalse();
        message.RetryCount.Should().Be(3);
        message.IsParked.Should().BeFalse();

        var parked = message.RecordFailure("last error", Now.AddMinutes(5), maxRetryCount, Initial, Max);

        parked.Should().BeTrue();
        message.IsParked.Should().BeTrue();
        message.ParkedAt.Should().Be(Now.AddMinutes(5));
        message.RetryCount.Should().Be(3, "parking does not schedule another retry");
        message.Error.Should().Be("last error");
        message.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void RecordFailure_ZeroMaxRetryCount_ParksOnTheFirstFailure()
    {
        var message = NewMessage();

        var parked = message.RecordFailure("e", Now, 0, Initial, Max);

        parked.Should().BeTrue();
        message.RetryCount.Should().Be(0);
        message.ParkedAt.Should().Be(Now);
    }

    [Fact]
    public void RecordFailure_OnParkedMessage_Throws()
    {
        var message = NewMessage();
        message.RecordFailure("e", Now, 0, Initial, Max);

        var act = () => message.RecordFailure("e", Now, 0, Initial, Max);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordFailure_OnProcessedMessage_Throws()
    {
        var message = NewMessage();
        message.MarkAsProcessed(Now);

        var act = () => message.RecordFailure("e", Now, 5, Initial, Max);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordFailure_BlankError_Throws()
    {
        var act = () => NewMessage().RecordFailure(" ", Now, 5, Initial, Max);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordFailure_InvalidPolicy_Throws()
    {
        var negativeRetries = () => NewMessage().RecordFailure("e", Now, -1, Initial, Max);
        var zeroInitial = () => NewMessage().RecordFailure("e", Now, 5, TimeSpan.Zero, Max);
        var maxBelowInitial = () => NewMessage().RecordFailure("e", Now, 5, Initial, TimeSpan.FromSeconds(5));

        negativeRetries.Should().Throw<ArgumentOutOfRangeException>();
        zeroInitial.Should().Throw<ArgumentOutOfRangeException>();
        maxBelowInitial.Should().Throw<ArgumentOutOfRangeException>();
    }
}
