using Microsoft.Extensions.Options;

namespace Worker;

public class OutboxPublisherOptions
{
    public const string SectionName = "OutboxPublisher";

    /// <summary>Maximum messages read per poll.</summary>
    public int BatchSize {get; set;} = 50;

    /// <summary>Wait between polls when nothing was published.</summary>
    public TimeSpan PollInterval {get; set;} = TimeSpan.FromSeconds(2);

    /// <summary>Wait after a poll fails as a whole, for example when the database is unreachable.</summary>
    public TimeSpan ErrorDelay {get; set;} = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Retries allowed after the first attempt. A message is attempted at most 1 + MaxRetryCount times,
    /// then parked. Zero means no retries: the first failure parks the message.
    /// </summary>
    public int MaxRetryCount {get; set;} = 10;

    /// <summary>Delay before the first retry. Each later retry doubles it.</summary>
    public TimeSpan InitialRetryDelay {get; set;} = TimeSpan.FromSeconds(5);

    /// <summary>Upper bound for the retry delay.</summary>
    public TimeSpan MaxRetryDelay {get; set;} = TimeSpan.FromMinutes(5);
}

public class OutboxPublisherOptionsValidator : IValidateOptions<OutboxPublisherOptions>
{
    public ValidateOptionsResult Validate(string? name, OutboxPublisherOptions options)
    {
        var failures = new List<string>();

        if (options.BatchSize < 1) failures.Add($"{OutboxPublisherOptions.SectionName}:{nameof(options.BatchSize)} must be at least 1.");
        if (options.PollInterval <= TimeSpan.Zero) failures.Add($"{OutboxPublisherOptions.SectionName}:{nameof(options.PollInterval)} must be greater than zero.");
        if (options.ErrorDelay < TimeSpan.Zero) failures.Add($"{OutboxPublisherOptions.SectionName}:{nameof(options.ErrorDelay)} cannot be negative.");
        if (options.MaxRetryCount < 0) failures.Add($"{OutboxPublisherOptions.SectionName}:{nameof(options.MaxRetryCount)} cannot be negative.");
        if (options.InitialRetryDelay <= TimeSpan.Zero) failures.Add($"{OutboxPublisherOptions.SectionName}:{nameof(options.InitialRetryDelay)} must be greater than zero.");
        if (options.MaxRetryDelay < options.InitialRetryDelay) failures.Add($"{OutboxPublisherOptions.SectionName}:{nameof(options.MaxRetryDelay)} must be greater than or equal to {nameof(options.InitialRetryDelay)}.");

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
