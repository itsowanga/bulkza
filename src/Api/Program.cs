using Application;
using Application.Interfaces;
using Infrastructure;
using Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SQL_SERVER_CONNECTION_STRING")!;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

var app = builder.Build();

app.MapControllers();
app.Run();