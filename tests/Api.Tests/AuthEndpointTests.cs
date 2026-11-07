using System.Net.Http.Json;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

[Collection("api")]
public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ApiSmokeTestFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;
    
    public AuthEndpointTests(ApiSmokeTestFixture fixture, WebApplicationFactory<Program> factory)
    {
        _fixture = fixture;
        _factory = factory;
        _fixture.ResetDatabaseAsync().GetAwaiter().GetResult();
        
    }

    [Fact]
    public async Task Register_WithoutASuburb_Succeeds_BecauseTheSuburbIsOptional()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";

        var register = await _fixture.Client.PostAsJsonAsync("/api/merchants/register", new
        {
            name = "No Suburb Traders",
            phoneNumber,
            password,
            street = "45 Voortrekker Rd",
            city = "Aliwal North",
            province = "Eastern Cape",
            postalCode = "9750",
            country = "South Africa"
        });

        register.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var (client, login) = await AuthHelper.LoginMerchant(_fixture, _factory, phoneNumber, password);
        login.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        client.Should().NotBeNull();
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
    public async Task Register_WithDuplicatePhoneNumber_ReturnsBadRequest()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";

        var firstResponse = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        firstResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var secondResponse = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        secondResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

    }

    [Fact]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ng";

        var response = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessMessage()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";

        var response = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var (client, message) = await AuthHelper.LoginMerchant(_fixture, _factory, phoneNumber, password);

        message.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        client!.DefaultRequestHeaders.Authorization.Should().NotBeNull();

    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";

        var response = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var (client, message) = await AuthHelper.LoginMerchant(_fixture, _factory, phoneNumber, "$tr0ngP@sswrd");
        message.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        client.Should().BeNull();

    }

    [Fact]
    public async Task Login_WithWrongPhone_ReturnsUnauthorized()
    {
        var phoneNumber = "0825547878";
        var password = "$tr0ngP@ssw0rd";

        var response = await AuthHelper.RegisterMerchant(_fixture, phoneNumber, password);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var (client, message) = await AuthHelper.LoginMerchant(_fixture, _factory, "0855689987", password);
        message.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        client.Should().BeNull();

    }
}