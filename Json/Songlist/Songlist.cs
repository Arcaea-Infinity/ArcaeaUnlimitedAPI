using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

#pragma warning disable CS8618

[Serializable]
public class Songlist
{
    [JsonProperty("songs")] public List<SongItem> Songs { get; set; }
}
