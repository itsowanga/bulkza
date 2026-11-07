namespace Domain;
public class OutboxMessage
{
    public Guid Id {get; init;}
    public string Type {get; private set;}
    public string Content {get; private set;}
    public DateTimeOffset OccuredAt  {get; private set;}
    public DateTimeOffset? ProcessedAt {get; private set;}
    public string? Error {get; private set;}
    public int RetryCount {get; private set;}
    public DateTimeOffset? NextAttemptOn {get; private set;}
    public DateTimeOffset? ParkedAt {get; private set;}

    private OutboxMessage(string type, string content, DateTimeOffset occuredAt, DateTimeOffset? processedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        if (occuredAt == default) throw new ArgumentException($"{nameof(occuredAt)} cannot be the default value.", nameof(occuredAt));
        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        OccuredAt = occuredAt;
        ProcessedAt = processedAt;
        Error = null;
        RetryCount = 0;
        NextAttemptOn = null;
        ParkedAt = null;
    }

    private OutboxMessage(Guid id, string type, string content, DateTimeOffset occuredAt,
                          int retry, DateTimeOffset? nextAttempt, DateTimeOffset? processedAt, string? error, DateTimeOffset? parkedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        if (occuredAt == default) throw new ArgumentException($"{nameof(occuredAt)} cannot be the default value.", nameof(occuredAt));
        Id = id;
        Type = type;
        Content = content;
        OccuredAt = occuredAt;
        RetryCount = retry;
        NextAttemptOn = nextAttempt;
        ProcessedAt = processedAt;
        Error = error;
        ParkedAt = parkedAt;

    }

    /// <summary>True once the retry limit was reached. A parked message is never published again.</summary>
    public bool IsParked => ParkedAt is not null;

    public static OutboxMessage Create(string type, string content, DateTimeOffset occuredAt, DateTimeOffset? processedAt) => new(type, content, occuredAt, processedAt);
    public static OutboxMessage From(Guid id, string type, string content, DateTimeOffset occuredAt, DateTimeOffset? processedAt,
                                     string? error, int retry, DateTimeOffset? nextAttempt, DateTimeOffset? parkedAt = null) => new(id, type, content, occuredAt, retry, nextAttempt, processedAt, error, parkedAt);
    public void MarkAsProcessed(DateTimeOffset time)
    {
        if (time == default) throw new ArgumentException($"{nameof(time)} cannot be the default value.", nameof(time));
        ProcessedAt = time;
    }

    /// <summary>
    /// Records a failed publish attempt. <paramref name="maxRetryCount"/> is the number of retries allowed after the
    /// first attempt, so a message is attempted at most <c>1 + maxRetryCount</c> times.
    /// Retry n is scheduled <c>initialRetryDelay * 2^(n-1)</c> after <paramref name="now"/>, capped at <paramref name="maxRetryDelay"/>.
    /// When no retries remain, the message is parked instead of rescheduled.
    /// </summary>
    /// <returns>True if the message was parked, false if a retry was scheduled.</returns>
    public bool RecordFailure(string error, DateTimeOffset now, int maxRetryCount, TimeSpan initialRetryDelay, TimeSpan maxRetryDelay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        if (now == default) throw new ArgumentException($"{nameof(now)} cannot be the default value.", nameof(now));
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetryCount);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(initialRetryDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRetryDelay, initialRetryDelay);
        if (ProcessedAt is not null) throw new InvalidOperationException("A processed message cannot record a failure.");
        if (ParkedAt is not null) throw new InvalidOperationException("A parked message cannot record a failure.");

        Error = error;

        if (RetryCount >= maxRetryCount)
        {
            ParkedAt = now;
            return true;
        }

        RetryCount += 1;
        var exponent = Math.Min(RetryCount - 1, 30);
        var seconds = Math.Min(initialRetryDelay.TotalSeconds * Math.Pow(2, exponent), maxRetryDelay.TotalSeconds);
        NextAttemptOn = now.AddSeconds(seconds);
        return false;
    }

}
