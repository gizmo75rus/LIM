using System.Text.Json;
using System.Text.Json.Serialization;

namespace LIM.WebApp.Extensions;

public static class HttpExtensions
{
    static readonly JsonSerializerOptions _settings = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  
    };

    public static async Task WriteJsonAsync<T>(this HttpResponse response, T obj)
    {
        response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(obj, _settings);

        await response.WriteAsync(json);
    }
}