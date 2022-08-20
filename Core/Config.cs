using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static ConfigItem Config;
    internal static List<string> UserAgents;
    internal static HashSet<string> Tokens;

    internal static volatile bool NeedUpdate = false;

    static GlobalConfig()
    {
        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("config.json"))!;
        UserAgents = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("useragents.json"))!;
        Tokens = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText("tokens.json"))!;

        ArcaeaFetch.Init();
        BackgroundService.Init();
        ConfigWatcher.Init();
    }

    internal static void Init(string fileName)
    {
        switch (fileName)
        {
            case "config.json":
                Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("config.json"))!;
                ArcaeaFetch.Init();
                break;
            case "useragents.json":
                UserAgents = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("useragents.json"))!;
                break;
            case "tokens.json":
                Tokens = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText("tokens.json"))!;
                break;
        }
    }
}

#pragma warning disable CS8618

public class ConfigItem
{
    [JsonProperty("api_entry")] public string ApiEntry { get; set; }

    [JsonProperty("app_version")] public string Appversion { get; set; }

    [JsonProperty("host")] public string Host { get; set; }

    [JsonProperty("cert_name")] public string CertFileName { get; set; }

    [JsonProperty("cert_password")] public string CertPassword { get; set; }

    [JsonProperty("data_path")] public string DataPath { get; set; }

    [JsonProperty("open_register")] public bool? OpenRegister { get; set; }
    [JsonProperty("quota")] public int Quota { get; set; } = 10;

    [JsonConverter(typeof(BytesConverter))] [JsonProperty("api_salt")]
    public byte[] ApiSalt { get; set; }

    [JsonProperty("node")] public Node Node { get; set; }

    internal void WriteConfig(bool rewrite) =>
        File.WriteAllText(rewrite
                              ? "config.json"
                              : "config_temp.json", JsonConvert.SerializeObject(this));
}

public class Node
{
    [JsonProperty("url")] public string Url { get; set; }
    [JsonProperty("port")] public int? Port { get; set; }

    public override string ToString() => $"https://{Url}:{Port ?? 443}";
}

internal class BytesConverter : JsonConverter<byte[]>
{
    public override bool CanWrite => true;
    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
    {
        if (value != null) writer.WriteValue(Convert.ToHexString(value));
    }

    public override byte[] ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue,
                                    JsonSerializer serializer)
    {
        var readerValue = reader.Value?.ToString();

        return readerValue == null
            ? hasExistingValue
                ? existingValue!
                : Array.Empty<byte>()
            : Convert.FromHexString(readerValue);
    }
}
