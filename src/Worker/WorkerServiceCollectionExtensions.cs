using Application.Interfaces.EventBus;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Worker;

public static class WorkerServiceCollectionExtensions
{
    public const string ConnectionStringName = "DefaultConnection";

    /// <summary>
    /// Registers the outbox publisher and everything it needs.
    /// Throws if the database connection string is missing. Options are validated when the host starts.
    /// </summary>
    public static IServiceCollection AddOutboxWorker(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{ConnectionStringName} is not configured. " +
                $"Set it with user secrets (dotnet user-secrets set \"ConnectionStrings:{ConnectionStringName}\" \"<value>\") " +
                $"or the ConnectionStrings__{ConnectionStringName} environment variable. Use the same value as the API.");
        }

        services.AddInfrastructure(connectionString);

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IEventBus, LoggingEventBus>();

        services.AddSingleton<IValidateOptions<OutboxPublisherOptions>, OutboxPublisherOptionsValidator>();
        services.AddOptions<OutboxPublisherOptions>()
                .Bind(configuration.GetSection(OutboxPublisherOptions.SectionName))
                .ValidateOnStart();

        services.AddHostedService<OutboxPublisher>();
        return services;
    }
}
