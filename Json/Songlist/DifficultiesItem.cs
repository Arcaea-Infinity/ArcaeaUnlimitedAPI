using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

[Serializable]
public class DifficultiesItem
{
    [JsonProperty("ratingClass")] public int RatingClass { get; set; }
    [JsonProperty("chartDesigner")] public string ChartDesigner { get; set; } = "";
    [JsonProperty("jacketDesigner")] public string JacketDesigner { get; set; } = "";

    [JsonProperty("jacketOverride")] public bool JacketOverride { get; set; }

    [JsonProperty("realrating")] public int RealRating { get; set; }
    [JsonProperty("totalNotes")] public int TotalNotes { get; set; }
}
