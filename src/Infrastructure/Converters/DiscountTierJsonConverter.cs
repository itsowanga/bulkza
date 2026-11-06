using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects;

namespace Infrastructure.Converters;
public class DiscountTierJsonConverter : JsonConverter<DiscountTier>
{
    public override DiscountTier Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var minimumQuantity = Quantity.Create(root.GetProperty("MinimumQuantity").GetInt32());
        var discountPercentage = root.GetProperty("DiscountPercentage").GetDecimal();

        return DiscountTier.Create(minimumQuantity, discountPercentage);
    }

    public override void Write(Utf8JsonWriter writer, DiscountTier value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("MinimumQuantity", value.MinimumQuantity.Value);
        writer.WriteNumber("DiscountPercentage", value.DiscountPercentage);
        writer.WriteEndObject();
    }
}