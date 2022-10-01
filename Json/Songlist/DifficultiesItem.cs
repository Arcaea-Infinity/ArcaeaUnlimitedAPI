using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

[Serializable]
public class DifficultiesItem
{
    [JsonProperty("ratingClass")]
    public int RatingClass { get; set; }

    [JsonProperty("chartDesigner")]
    public string ChartDesigner { get; set; } = string.Empty;

    [JsonProperty("jacketDesigner")]
    public string JacketDesigner { get; set; } = string.Empty;

    [JsonProperty("jacketOverride")]
    public bool JacketOverride { get; set; }

    [JsonProperty("audioOverride")]
    public bool AudioOverride { get; set; }

    [JsonProperty("title_localized")]
    public TitleLocalized? TitleLocalized { get; set; }

    [JsonProperty("artist")]
    public string? Artist { get; set; }

    [JsonProperty("bpm")]
    public string? Bpm { get; set; }

    [JsonProperty("bpm_base")]
    public double? BpmBase { get; set; }

    [JsonProperty("set")]
    public string? Set { get; set; }

    [JsonProperty("world_unlock")]
    public bool? WorldUnlock { get; set; }

    [JsonProperty("remote_dl")]
    public bool? NeedDownload { get; set; }

    [JsonProperty("side")]
    public int? Side { get; set; }

    [JsonProperty("date")]
    public long? Date { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("bg")]
    public string? Background { get; set; }

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("ratingPlus")]
    public bool? RatingPlus { get; set; }
}
