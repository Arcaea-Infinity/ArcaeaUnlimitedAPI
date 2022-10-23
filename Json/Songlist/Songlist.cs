using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

#pragma warning disable CS8618

[Serializable]
public sealed class Songlist
{
    [JsonProperty("songs")]
    public List<SongItem> Songs { get; set; }
}
