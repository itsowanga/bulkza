using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Tests;

/// <summary>Options that read the API's JSON: camelCase names and enums as strings, like the real clients do.</summary>
public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
