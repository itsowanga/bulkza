using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public static class AuthHelper
{

    public static async Task<HttpResponseMessage> RegisterMerchant(ApiSmokeTestFixture fixture, string phoneNumber, string password)
    {

        var response = await fixture.Client.PostAsJsonAsync("/api/merchants/register", new
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

        return response;
    }

    public static async Task<(HttpClient? client, HttpResponseMessage message)> LoginMerchant(ApiSmokeTestFixture fixture, WebApplicationFactory<Program> factory, string phoneNumber, string password)
    {

        var response = await fixture.Client.PostAsJsonAsync("/api/merchants/login", new
        {
            phoneNumber,
            password
        });

        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return (null, response);
        }
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Login returned no body.");

        var authenticated = factory.CreateClient();
        authenticated.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result.Token);


        return (authenticated, response);
    }

    private sealed record LoginResponse(string Token);
}