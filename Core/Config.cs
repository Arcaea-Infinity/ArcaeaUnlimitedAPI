using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static readonly ConfigItem Config;

    internal static volatile bool NeedUpdate = false;

    static GlobalConfig()
    {
        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("config.json"))!;

        ArcaeaFetch.Init();
        BackgroundService.Init();
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

    [JsonProperty("root_path")] public string DataRootPath { get; set; }

    [JsonProperty("open_register")] public bool? OpenRegister { get; set; }

    [JsonProperty("api_salt")] public byte[] ApiSalt { get; set; }

    [JsonProperty("nodes")] public List<Node> Nodes { get; set; }

    [JsonProperty("whitelist")] public List<string> Whitelist { get; set; }

    internal void WriteConfig(bool rewrite) =>
        File.WriteAllText(rewrite
                              ? "config.json"
                              : "config_temperate.json", JsonConvert.SerializeObject(this));
}

public class Node
{
    [JsonProperty("url")] public string Url { get; set; }
    [JsonProperty("port")] public int? Port { get; set; }
    [JsonProperty("active")] internal bool Active { get; set; } = true;

    public override string ToString() => $"https://{Url}:{Port ?? 443}";
}
