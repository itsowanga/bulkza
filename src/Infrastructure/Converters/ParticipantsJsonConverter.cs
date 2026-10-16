using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects;

namespace Infrastructure.Converters;
public class ParticipantsJsonConverter : JsonConverter<Participant>
{
    public override Participant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var merchantId = MerchantId.Create(root.GetProperty("MerchantId").GetGuid());
        var quantity = Quantity.Create(root.GetProperty("Quantity").GetInt32());

        return Participant.Create(merchantId, quantity);
    }

    public override void Write(Utf8JsonWriter writer, Participant value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("MerchantId", value.MerchantId.Value);
        writer.WriteNumber("Quantity", value.Quantity.Value);
        writer.WriteEndObject();
    }
}