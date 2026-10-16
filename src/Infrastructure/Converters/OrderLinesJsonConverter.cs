using System.Text.Json;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Converters;
public class OrderLinesJsonConverter : ValueConverter<IReadOnlyList<OrderLine>, string>
{
    
    public static readonly JsonSerializerOptions JsonOptions = new();

    public OrderLinesJsonConverter() : base( orderlines => JsonSerializer.Serialize(orderlines, JsonOptions),
                                             json => DeserializeOrderLines(json)){}

    
}
