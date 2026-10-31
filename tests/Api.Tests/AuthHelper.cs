using System.Net.Http.Json;

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
}