using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

#pragma warning disable CS8618

[Serializable]
public class SongItem
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title_localized")]
    public TitleLocalized TitleLocalized { get; set; }

    [JsonProperty("artist")]
    public string Artist { get; set; } = string.Empty;

    [JsonProperty("bpm")]
    public string Bpm { get; set; } = string.Empty;

    [JsonProperty("bpm_base")]
    public double BpmBase { get; set; }

    [JsonProperty("set")]
    public string Set { get; set; } = string.Empty;

    [JsonProperty("world_unlock")]
    public bool WorldUnlock { get; set; }

    [JsonProperty("remote_dl")]
    public bool NeedDownload { get; set; }

    [JsonProperty("side")]
    public int Side { get; set; }

    [JsonProperty("bg")]
    public string Background { get; set; } = string.Empty;

    [JsonProperty("time")]
    public int Time { get; set; }

    [JsonProperty("date")]
    public long Date { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("difficulties")]
    public List<DifficultiesItem> Difficulties { get; set; }
}
