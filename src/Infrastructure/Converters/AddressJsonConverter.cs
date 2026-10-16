using System.Text.Json;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Converters;

public class AddressJsonConverter : ValueConverter<Address, string>
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public AddressJsonConverter() : base( address => JsonSerializer.Serialize(address, JsonOptions),
                                          json => DeserializeAddress(json)
    ){}

    private static Address DeserializeAddress(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var suburb = root.TryGetProperty("Suburb", out var suburbElement)
                     && suburbElement.ValueKind != JsonValueKind.Null
                     ? suburbElement.GetString() : null;
        return Address.Create(root.GetProperty("Street").GetString()!,
                              root.GetProperty("City").GetString()!,
                              root.GetProperty("Province").GetString()!,
                              root.GetProperty("PostalCode").GetString()!,
                              root.GetProperty("Country").GetString()!,
                              suburb);
    }
}