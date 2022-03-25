using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

public class ResponseRoot
{
    [JsonProperty("success")] public bool Success { get; set; }
    [JsonProperty("code")] public string? Code { get; set; }
    [JsonProperty("error_code")] public int? ErrorCode { get; set; }
    [JsonProperty("access_token")] public string? AccessToken { get; set; }
    [JsonProperty("value")] public dynamic? Value { get; set; }

    internal T DeserializeContent<T>() => JsonConvert.DeserializeObject<T>(Value!.ToString(), new Int32Converter());
}

internal class Int32Converter : JsonConverter<int?>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, int? value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override int? ReadJson(JsonReader reader, Type objectType, int? existingValue, bool hasExistingValue,
                                  JsonSerializer serializer)
    {
        var readerValue = reader.Value?.ToString();
        if (readerValue == null) return 0;
        int.TryParse(readerValue, out var value);
        return value;
    }
}
