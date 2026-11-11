using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using Api.Seeding;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Api.Tests;

[Collection("api")]
public class AdminSeederTests
{
    private const string Phone = "0825550002";
    private const string Password = "Seed3d!Passw0rd";

    private readonly ApiSmokeTestFixture _fixture;

    public AdminSeederTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    private AdminSeeder CreateSeeder(string? phone, string? password) =>
        new(_fixture.Services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new AdminSeedOptions { Phone = phone, Password = password }),
            NullLogger<AdminSeeder>.Instance);

    private async Task<int> CountMerchantsAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<BulkzaDbContext>().Merchants.CountAsync();
    }

    private async Task<string?> LoginRoleAsync(string phone, string password)
    {
        var response = await _fixture.Client.PostAsJsonAsync("/api/merchants/login", new { phoneNumber = phone, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginBody>();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(body!.Token);
        return token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
    }

    [Fact]
    public async Task StartAsync_WithConfiguredCredentials_CreatesAnAdminWhoCanLogInWithTheAdminRole()
    {
        await CreateSeeder(Phone, Password).StartAsync(CancellationToken.None);

        (await CountMerchantsAsync()).Should().Be(1);
        (await LoginRoleAsync(Phone, Password)).Should().Be("Admin");
    }

    [Fact]
    public async Task StartAsync_WhenRunTwice_DoesNotCreateASecondAdmin()
    {
        await CreateSeeder(Phone, Password).StartAsync(CancellationToken.None);
        await CreateSeeder(Phone, Password).StartAsync(CancellationToken.None);

        (await CountMerchantsAsync()).Should().Be(1);
    }

    [Fact]
    public async Task StartAsync_WithNothingConfigured_SkipsSeeding()
    {
        await CreateSeeder(null, null).StartAsync(CancellationToken.None);

        (await CountMerchantsAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(Phone, null)]
    [InlineData(null, Password)]
    [InlineData(Phone, "  ")]
    public async Task StartAsync_WithOnlyHalfConfigured_FailsStartup(string? phone, string? password)
    {
        var act = () => CreateSeeder(phone, password).StartAsync(CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("AdminSeed:Phone");
        (await CountMerchantsAsync()).Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_WithInvalidPhoneNumber_FailsStartupAndCreatesNothing()
    {
        var act = () => CreateSeeder("12345", Password).StartAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        (await CountMerchantsAsync()).Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_WhenThePhoneBelongsToAnOrdinaryMerchant_DoesNotPromoteThemOrChangeTheirPassword()
    {
        var registered = await AuthHelper.RegisterMerchant(_fixture, Phone, "Or!dinaryPassw0rd");
        registered.EnsureSuccessStatusCode();

        await CreateSeeder(Phone, Password).StartAsync(CancellationToken.None);

        (await CountMerchantsAsync()).Should().Be(1);
        (await LoginRoleAsync(Phone, "Or!dinaryPassw0rd")).Should().Be("Merchant");
        var wrongPassword = await _fixture.Client.PostAsJsonAsync("/api/merchants/login", new { phoneNumber = Phone, password = Password });
        wrongPassword.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    private sealed record LoginBody(string Token);
}
