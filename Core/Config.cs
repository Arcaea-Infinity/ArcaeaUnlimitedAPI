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

    [JsonProperty("nodes")] public List<Node> Nodes { get; set; }

    [JsonProperty("whitelist")] public List<string> Whitelist { get; set; }
    
    [JsonProperty("challenge_type")] public string ChallengeType { get; set; }
    [JsonProperty("challenge_api")] public string ChallengeUrl { get; set; }
    [JsonProperty("challenge_token")] public string ChallengeToken { get; set; }
}

public class Node
{
    [JsonProperty("url")] public string Url { get; set; }
    [JsonProperty("port")] public int? Port { get; set; }
    [JsonProperty("active")] internal bool Active { get; set; } = true;

    public override string ToString() => $"https://{Url}:{Port ?? 443}";
}
