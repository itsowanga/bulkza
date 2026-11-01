using System.Net.Http.Json;
using Api.Controllers.BulkPools;
using Application;
using Application.BulkPools;
using Domain;

namespace Api.Tests;
public class BulkPoolHelper
{
    public static async Task<PagedResult<BulkPoolDto>> GetMyPoolsAsync(HttpClient client, int page, int pageSize)
    {
        var response = await client.GetAsync($"/api/bulkpools/mine?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();  

        var result = await response.Content.ReadFromJsonAsync<PagedResult<BulkPoolDto>>();
        return result ?? throw new InvalidOperationException("Response body was empty.");
    }
    public static async Task<HttpResponseMessage> CreatePoolAsync(HttpClient client, Guid product, int minimumQuantity, int minimumMerchants, DateTimeOffset deadline)
    {
        
        var response = await client.PostAsJsonAsync("/api/bulkpools", new
        {
            ProductId = product,
            MinimumQuantity = minimumQuantity,
            MinimumMerchants = minimumMerchants,
            Deadline = deadline
        });
        
        return response;

    }
}