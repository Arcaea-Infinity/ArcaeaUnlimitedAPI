using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static ConfigItem Config = null!;
    internal static HashSet<string> Tokens = null!;

    internal static volatile bool NeedUpdate = false;
    internal static volatile bool IllegalHash = false;

    static GlobalConfig()
    {
        Init();
        DatabaseManager.Init();
        ArcaeaFetch.Init();
        BackgroundService.Init();
        ConfigWatcher.Init();
    }

    private static void Init()
    {
        var apiconfig = Path.Combine(AppContext.BaseDirectory, "apiconfig.json");
        if (!File.Exists(apiconfig)) File.WriteAllText(apiconfig, JsonConvert.SerializeObject(new ConfigItem(), Formatting.Indented));
        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;

        var tokens = Path.Combine(AppContext.BaseDirectory, "tokens.json");
        if (!File.Exists(tokens)) File.WriteAllText(tokens, "[]");
        Tokens = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText("tokens.json"))!;

        Directory.CreateDirectory(Path.Combine(Config.DataPath, "log"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "database"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "update"));
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
    public string ApiEntry { get; set; } = string.Empty;

    [JsonProperty("app_version")]
    public string Appversion { get; set; } = string.Empty;

    [JsonProperty("host")]
    public string Host { get; set; } = string.Empty;

    [JsonProperty("cert_name")]
    public string CertFileName { get; set; } = string.Empty;

    [JsonProperty("cert_password")]
    public string CertPassword { get; set; } = string.Empty;

    [JsonProperty("data_path")]
    public string DataPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data");

    [JsonProperty("open_register")]
    public bool? OpenRegister { get; set; } = false;

    [JsonProperty("quota")]
    public int Quota { get; set; } = 10;

    [JsonProperty("challenge_type")]
    public string ChallengeType { get; set; } = string.Empty;

    [JsonProperty("challenge_api")]
    public string ChallengeUrl { get; set; } = string.Empty;

    [JsonProperty("challenge_token")]
    public string ChallengeToken { get; set; } = string.Empty;
 
    [JsonProperty("nodes")]
    public List<Node> Nodes { get; set; } = new() { new() };
    
    internal void WriteConfig() => File.WriteAllText("apiconfig.json", JsonConvert.SerializeObject(this, Formatting.Indented));
}

public class Node
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("port")]
    public int? Port { get; set; } = 443;

    [JsonProperty("active")]
    internal bool Active { get; set; } = true;

    public override string ToString() => $"https://{Url}:{Port ?? 443}";
}
