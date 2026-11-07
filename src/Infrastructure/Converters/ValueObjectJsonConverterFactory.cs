using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Converters;

/// <summary>
/// Reads and writes single-value domain value objects (PoolId, MerchantId, EventId, Quantity, ...)
/// as <c>{ "Value": ... }</c>. These types have private constructors and read-only properties,
/// so the default System.Text.Json deserialiser returns default(T) for them.
/// The converter rebuilds them through their public static <c>Create</c> method so validation still runs.
/// </summary>
public sealed class ValueObjectJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => TryGetShape(typeToConvert, out _, out _);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!TryGetShape(typeToConvert, out var valueProperty, out var create)) return null;

        var converterType = typeof(ValueObjectJsonConverter<,>).MakeGenericType(typeToConvert, valueProperty.PropertyType);
        return (JsonConverter)Activator.CreateInstance(converterType, valueProperty, create)!;
    }

    private static bool TryGetShape(Type type, out PropertyInfo valueProperty, out MethodInfo create)
    {
        valueProperty = null!;
        create = null!;

        if (!type.IsValueType || type.Namespace != "Domain.ValueObjects") return false;

        var property = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        if (property is null) return false;

        var factory = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, [property.PropertyType]);
        if (factory is null || factory.ReturnType != type) return false;

        valueProperty = property;
        create = factory;
        return true;
    }

    private sealed class ValueObjectJsonConverter<T, TValue>(PropertyInfo valueProperty, MethodInfo create) : JsonConverter<T>
        where T : struct
    {
        // Struct instance getters can't be bound with Delegate.CreateDelegate, so compile an expression instead.
        private readonly Func<T, TValue> _getValue = BuildGetter(valueProperty);
        private readonly Func<TValue, T> _create = (Func<TValue, T>)Delegate.CreateDelegate(typeof(Func<TValue, T>), create);

        private static Func<T, TValue> BuildGetter(PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(T), "value");
            return Expression.Lambda<Func<T, TValue>>(Expression.Property(parameter, property), parameter).Compile();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var value = doc.RootElement.GetProperty("Value").Deserialize<TValue>(options)
                        ?? throw new JsonException($"{typeof(T).Name}.Value is null.");
            return _create(value);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, _getValue(value), options);
            writer.WriteEndObject();
        }
    }
}
