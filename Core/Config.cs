using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static ConfigItem Config = null!;

    internal static volatile bool NeedUpdate = false;

    static GlobalConfig()
    {
        Init();
        BackgroundService.Init();
        ConfigWatcher.Init();
    }

    internal static void Init()
    {
        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("config.json"))!;
        ArcaeaFetch.Init();
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

    [JsonProperty("api_salt")] public List<byte> ApiSalt { get; set; }
    
    [JsonProperty("node")] public Node Node { get; set; }
    
    [JsonProperty("whitelist")] public List<string> Whitelist { get; set; }

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
