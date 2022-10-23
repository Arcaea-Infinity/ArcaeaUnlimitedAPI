using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

#pragma warning disable CS8618

[Serializable]
public sealed class TitleLocalized
{
    [JsonProperty("en")]
    public string En { get; set; } = string.Empty;

    [JsonProperty("ja")]
    public string? Ja { get; set; }
}
