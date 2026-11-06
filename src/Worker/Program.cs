using Infrastructure;
using Worker;
using System.Threading.Channels;
using Application.Interfaces.EventBus;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddScoped<IEventBus, InMemoryServiceBus>();
builder.Services.Configure<OutboxPublisherOptions>(builder.Configuration.GetSection("OutboxPublisher"));
builder.Services.AddHostedService<OutboxPublisher>();

var host = builder.Build();
host.Run();
