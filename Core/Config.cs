using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static ConfigItem Config;
    internal static HashSet<string> Tokens;

    internal static volatile bool NeedUpdate = false;
    internal static volatile bool IllegalHash = false;

    static GlobalConfig()
    {
        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;
        Tokens = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText("tokens.json"))!;

        CreateDirectory();

        DatabaseManager.Init();
        ArcaeaFetch.Init();
        BackgroundService.Init();
        ConfigWatcher.Init();
    }

    private static void CreateDirectory()
    {
        Directory.CreateDirectory(Config.DataPath);
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "log"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "database"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "update"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "source"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "source", "songs"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "source", "char"));

        var arcversion = Path.Combine(Config.DataPath, "arcversion");
        if (!File.Exists(arcversion)) File.WriteAllText(arcversion, "4.0.0c");
    }

    internal static void Init(string fileName)
    {
        switch (fileName)
        {
            case "apiconfig.json":
                Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;
                ArcaeaFetch.Init();
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
    
    internal void WriteConfig() => File.WriteAllText("apiconfig.json", JsonConvert.SerializeObject(this, Formatting.Indented));
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
