namespace Domain;
public class OutboxMessage
{
    public Guid Id {get; init;}
    public string Type {get; private set;}
    public string Content {get; private set;}
    public DateTimeOffset OccuredAt  {get; private set;}
    public DateTimeOffset? ProcessedOn {get; private set;}
    public string? Error {get; private set;}
    public int RetryCount {get; private set;}
    public DateTimeOffset? NextAttemptOn {get; private set;}

    private OutboxMessage(string type, string content, DateTimeOffset occuredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        if (occuredAt == default) throw new ArgumentException($"{nameof(occuredAt)} cannot be the default value.", nameof(occuredAt));
        Id = Guid.NewGuid();
        Type = type;
        Content = content;
        OccuredAt = occuredAt;
        ProcessedOn = null;
        Error = null;
        RetryCount = 0;
        NextAttemptOn = null;
    }
    public static OutboxMessage Create(string type, string content, DateTimeOffset occuredAt) => new(type, content, occuredAt);
    public void MarkAsProcessed(DateTimeOffset time)
    {
        if (time == default) throw new ArgumentException($"{nameof(time)} cannot be the default value.", nameof(time));
        ProcessedOn = time;
    } 
    public void RecordFailure(string error)
    { 
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        int baseDelay = 5;
        int maxDelay = 60;
        RetryCount+=1;
        var seconds = Math.Min(baseDelay * Math.Pow(2, RetryCount), maxDelay);
        NextAttemptOn = DateTimeOffset.UtcNow.AddSeconds(seconds);
    }

}