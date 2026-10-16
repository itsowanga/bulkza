using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects;

namespace Infrastructure.Converters;
public class OrderLineJsonConverter : JsonConverter<OrderLine>
{
    public override OrderLine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var productId = ProductId.Create(root.GetProperty("ProductId").GetGuid());
        var quantity = Quantity.Create(root.GetProperty("Quantity").GetInt32());
        var unitPrice = Money.Create(root.GetProperty("UnitPrice").GetDecimal());

        return OrderLine.Create(productId, quantity, unitPrice);
    }

    public override void Write(Utf8JsonWriter writer, OrderLine value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("ProductId", value.ProductId.Value);
        writer.WriteNumber("Quantity", value.Quantity.Value);
        writer.WriteNumber("UnitPrice", value.UnitPrice.Amount);
        writer.WriteEndObject();
    }
}