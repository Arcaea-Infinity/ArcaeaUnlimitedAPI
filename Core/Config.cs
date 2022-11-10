using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

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
