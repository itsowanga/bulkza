using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Api.Tests;

public sealed class ApiSmokeTestFixture : IAsyncLifetime
{
    private MsSqlContainer _container = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private string _connectionString = null!;

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory.Services;

    [Obsolete]
    public async Task InitializeAsync()
    {

        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .Build();

        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();


        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<BulkzaDbContext>));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddDbContext<BulkzaDbContext>(options =>
                        options.UseSqlServer(_connectionString));
                });
            });

        Client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string phoneNumber, string password)
    {

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/merchants/login",
            new { phoneNumber, password });

        if (!loginResponse.IsSuccessStatusCode)
        {
            var registerResponse = await Client.PostAsJsonAsync(
                "/api/merchants/register",
                new
                {
                    name = "Test Merchant",
                    phoneNumber,
                    password,
                    street = "45 Voortrekker Rd",
                    city = "Aliwal North",
                    province = "Northern Cape",
                    postalCode = "7778",
                    country = "South Africa",
                    suburb = "Daveyton"
                });

            registerResponse.EnsureSuccessStatusCode();

            loginResponse = await Client.PostAsJsonAsync(
                "/api/merchants/login",
                new { phoneNumber, password });

            loginResponse.EnsureSuccessStatusCode();
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Login returned no body.");

        var authenticated = _factory.CreateClient();
        authenticated.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        return authenticated;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    private sealed record LoginResponse(string Token);
}