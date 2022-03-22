using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.Songlist;

#pragma warning disable CS8618

[Serializable]
internal class Songlist
{
    [JsonProperty("songs")] public List<SongsItem> Songs { get; set; }
}
