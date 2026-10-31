using System.Net.Http.Json;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

[Collection("api")]
public class AuthEndpointTests
{
    private readonly ApiSmokeTestFixture _fixture;
    
    public AuthEndpointTests(ApiSmokeTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Register_WithValidInput_ReturnsSuccessAndPersistsMerchant()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";
        var response = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        var merchant = await db.Merchants.SingleAsync(merchant => merchant.PhoneNumber == PhoneNumber.Create(phoneNumber));
        merchant.Should().NotBeNull();
        merchant.PasswordHash.Password.Should().NotBe("$tr0ngP@ssw0rd");
        merchant.PasswordHash.Password.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithDuplicatePhoneNumber_ReturnsConflictError()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";

        var firstResponse = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        firstResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var secondResponse = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        secondResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);

    }
}