﻿using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static ConfigItem Config;
    internal static List<string> UserAgents;
    internal static HashSet<string> Tokens;

    internal static volatile bool NeedUpdate = false;
    internal static volatile bool IllegalHash = false;

    static GlobalConfig()
    {
        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;
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
            case "apiconfig.json":
                Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;
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
    [JsonProperty("api_entry")]
    public string ApiEntry { get; set; }

    [JsonProperty("app_version")]
    public string Appversion { get; set; }

    [JsonProperty("host")]
    public string Host { get; set; }

    [JsonProperty("cert_name")]
    public string CertFileName { get; set; }

    [JsonProperty("cert_password")]
    public string CertPassword { get; set; }

    [JsonProperty("data_path")]
    public string DataPath { get; set; }

    [JsonProperty("open_register")]
    public bool? OpenRegister { get; set; }

    [JsonProperty("quota")]
    public int Quota { get; set; } = 10;

    [JsonProperty("challenge_type")]
    public string ChallengeType { get; set; }

    [JsonProperty("challenge_api")]
    public string ChallengeUrl { get; set; }

    [JsonProperty("challenge_token")]
    public string ChallengeToken { get; set; }

    [JsonProperty("nodes")]
    public List<Node> Nodes { get; set; }
}

public class Node
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("port")]
    public int? Port { get; set; }

    [JsonProperty("active")]
    internal bool Active { get; set; } = true;

    public override string ToString() => $"https://{Url}:{Port ?? 443}";
}
