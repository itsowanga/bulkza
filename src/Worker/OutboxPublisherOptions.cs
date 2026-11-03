namespace Worker;

public class OutboxPublisherOptions
{
    public int BatchSize {get; set;} = 50;
    public TimeSpan PollInterval {get; set;} = TimeSpan.FromSeconds(2);
    public TimeSpan ErrorDelay {get; set;} = TimeSpan.FromSeconds(5);
}