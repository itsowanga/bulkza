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
    }

    private OutboxMessage(Guid id, string type, string content, DateTimeOffset occuredAt,
                          int retry, DateTimeOffset? nextAttempt, DateTimeOffset? processedAt, string? error)
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

    }
    public static OutboxMessage Create(string type, string content, DateTimeOffset occuredAt, DateTimeOffset? processedAt) => new(type, content, occuredAt, processedAt);
    public static OutboxMessage From(Guid id, string type, string content, DateTimeOffset occuredAt, DateTimeOffset? processedAt,
                                     string? error, int retry, DateTimeOffset? nextAttempt) => new(id, type, content, occuredAt, retry, nextAttempt, processedAt, error);
    public void MarkAsProcessed(DateTimeOffset time)
    {
        if (time == default) throw new ArgumentException($"{nameof(time)} cannot be the default value.", nameof(time));
        ProcessedAt = time;
    } 
    public void RecordFailure(string error)
    { 
        if(RetryCount == 20)
        {
            NextAttemptOn = DateTimeOffset.MaxValue;
            return;
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        Error = error;
        int baseDelay = 5;
        int maxDelay = 60;
        RetryCount+=1;
        var seconds = Math.Min(baseDelay * Math.Pow(2, RetryCount), maxDelay);
        NextAttemptOn = DateTimeOffset.UtcNow.AddSeconds(seconds);
    }

}