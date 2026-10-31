using System.Net.Http.Json;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

[Collection("api")]
public class AuthEndpointTests (ApiSmokeTestFixture fixture)
{
    private readonly ApiSmokeTestFixture _fixture = fixture;

    [Fact]
    public async Task Register_WithValidInput_ReturnsSuccessAndPersistsMerchant()
    {
        var phoneNumber = "0825547878";

        var response = await _fixture.Client.PostAsJsonAsync("/api/merchants/register", new
        {
            name = "Test Merchant",
            phoneNumber,
            password = "$tr0ngP@ssw0rd",
            street = "45 Voortrekker Rd",
            city = "Aliwal North",
            province = "Northern Cape",
            postalCode = "7778",
            country = "South Africa",
            suburb = "Daveyton"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BulkzaDbContext>();
        var merchant = await db.Merchants.SingleAsync(merchant => merchant.PhoneNumber == PhoneNumber.Create(phoneNumber));
        merchant.Should().NotBeNull();
        merchant.PasswordHash.Password.Should().NotBe("$tr0ngP@ssw0rd");
        merchant.PasswordHash.Password.Should().NotBeNullOrWhiteSpace();
    }
}