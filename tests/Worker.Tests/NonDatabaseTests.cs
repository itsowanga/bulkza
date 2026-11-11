using Application.Interfaces.EventBus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Worker.Tests;

public class LoggingEventBusTests
{
    [Fact]
    public async Task PublishAsync_LogsEventTypeAndRawContent_WithoutAnyBrokerConfiguration()
    {
        var logger = new ListLogger<LoggingEventBus>();
        IEventBus bus = new LoggingEventBus(logger);

        await bus.PublishAsync("Domain.Events.PoolLocked, Domain", """{ "PoolId": { "Value": "abc" } }""");

        var entry = logger.Entries.Should().ContainSingle().Subject;
        entry.Level.Should().Be(LogLevel.Information);
        entry.Values["EventType"].Should().Be("Domain.Events.PoolLocked, Domain");
        entry.Values["EventContent"].Should().Be("""{ "PoolId": { "Value": "abc" } }""");
    }

    [Fact]
    public async Task PublishAsync_CancelledToken_ThrowsAndLogsNothing()
    {
        var logger = new ListLogger<LoggingEventBus>();
        var bus = new LoggingEventBus(logger);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = () => bus.PublishAsync("t", "c", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        logger.Entries.Should().BeEmpty();
    }
}

public class WorkerConfigurationTests
{
    private static IConfiguration Config(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
            .Build();

    private const string ValidConnection = "Server=localhost,1433;Database=bulkza;User Id=sa;Password=not-a-real-password;TrustServerCertificate=True;";

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddOutboxWorker_MissingConnectionString_ThrowsMeaningfulError(string? connectionString)
    {
        var services = new ServiceCollection();

        var act = () => services.AddOutboxWorker(Config(("ConnectionStrings:DefaultConnection", connectionString)));

        act.Should().Throw<InvalidOperationException>()
           .Which.Message.Should().Contain("ConnectionStrings:DefaultConnection")
                              .And.Contain("ConnectionStrings__DefaultConnection")
                              .And.Contain("user secrets");
    }

    [Fact]
    public void AddOutboxWorker_NoConnectionStringSectionAtAll_Throws()
    {
        var act = () => new ServiceCollection().AddOutboxWorker(Config());

        act.Should().Throw<InvalidOperationException>().Which.Message.Should().Contain("not configured");
    }

    [Fact]
    public void AddOutboxWorker_ValidConfiguration_UsesLoggingEventBusAndBindsOptions()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddOutboxWorker(Config(
            ("ConnectionStrings:DefaultConnection", ValidConnection),
            ("OutboxPublisher:MaxRetryCount", "7"),
            ("OutboxPublisher:InitialRetryDelay", "00:00:03"),
            ("OutboxPublisher:MaxRetryDelay", "00:02:00")));

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        provider.GetRequiredService<IEventBus>().Should().BeOfType<LoggingEventBus>();
        var options = provider.GetRequiredService<IOptions<OutboxPublisherOptions>>().Value;
        options.MaxRetryCount.Should().Be(7);
        options.InitialRetryDelay.Should().Be(TimeSpan.FromSeconds(3));
        options.MaxRetryDelay.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public void AddOutboxWorker_PublisherCanBeResolvedWithScopeValidation_SoItDoesNotCaptureScopedServices()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddOutboxWorker(Config(("ConnectionStrings:DefaultConnection", ValidConnection)));

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        provider.GetServices<IHostedService>().Should().ContainSingle(s => s is OutboxPublisher);
    }

    [Fact]
    public void Options_InvalidRetryConfiguration_FailsWithAMessageNamingEveryProblem()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddOutboxWorker(Config(
            ("ConnectionStrings:DefaultConnection", ValidConnection),
            ("OutboxPublisher:BatchSize", "0"),
            ("OutboxPublisher:MaxRetryCount", "-1"),
            ("OutboxPublisher:InitialRetryDelay", "00:01:00"),
            ("OutboxPublisher:MaxRetryDelay", "00:00:10")));
        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptions<OutboxPublisherOptions>>().Value;

        var message = act.Should().Throw<OptionsValidationException>().Which.Message;
        message.Should().Contain("BatchSize").And.Contain("MaxRetryCount").And.Contain("MaxRetryDelay");
    }

    [Fact]
    public async Task Host_InvalidOptions_FailsAtStartup()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ValidConnection,
            ["OutboxPublisher:InitialRetryDelay"] = "00:00:00",
        });
        builder.Services.AddOutboxWorker(builder.Configuration);
        using var host = builder.Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>();
    }
}

public class OutboxPublisherOptionsValidatorTests
{
    private readonly OutboxPublisherOptionsValidator _validator = new();

    [Fact]
    public void Defaults_AreValid() => _validator.Validate(null, new OutboxPublisherOptions()).Succeeded.Should().BeTrue();

    [Theory]
    [InlineData(nameof(OutboxPublisherOptions.BatchSize), 0)]
    [InlineData(nameof(OutboxPublisherOptions.MaxRetryCount), -1)]
    public void IntSettings_OutOfRange_Fail(string setting, int value)
    {
        var options = new OutboxPublisherOptions();
        typeof(OutboxPublisherOptions).GetProperty(setting)!.SetValue(options, value);

        var result = _validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain(setting);
    }

    [Fact]
    public void ZeroMaxRetryCount_IsAllowed_MeaningNoRetries() =>
        _validator.Validate(null, new OutboxPublisherOptions { MaxRetryCount = 0 }).Succeeded.Should().BeTrue();

    [Fact]
    public void NonPositivePollInterval_Fails() =>
        _validator.Validate(null, new OutboxPublisherOptions { PollInterval = TimeSpan.Zero }).Failed.Should().BeTrue();

    [Fact]
    public void NonPositiveInitialRetryDelay_Fails() =>
        _validator.Validate(null, new OutboxPublisherOptions { InitialRetryDelay = TimeSpan.Zero }).Failed.Should().BeTrue();

    [Fact]
    public void MaxRetryDelayBelowInitial_Fails() =>
        _validator.Validate(null, new OutboxPublisherOptions { InitialRetryDelay = TimeSpan.FromSeconds(10), MaxRetryDelay = TimeSpan.FromSeconds(5) })
                  .Failed.Should().BeTrue();

    [Fact]
    public void MaxRetryDelayEqualToInitial_IsValid() =>
        _validator.Validate(null, new OutboxPublisherOptions { InitialRetryDelay = TimeSpan.FromSeconds(5), MaxRetryDelay = TimeSpan.FromSeconds(5) })
                  .Succeeded.Should().BeTrue();
}
