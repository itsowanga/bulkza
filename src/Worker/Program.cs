using Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOutboxWorker(builder.Configuration);

var host = builder.Build();
host.Run();
