using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.Json.Songlist;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

public class UserBestResponse
{
    [JsonProperty("account_info")] public FriendsItem AccountInfo { get; set; }

    [JsonProperty("record")] public Records Record { get; set; }

    [JsonProperty("songinfo")] public SongsItem[]? Songinfo { get; set; }

    [JsonProperty("recent_score")] public Records? RecentScore { get; set; }

    [JsonProperty("recent_songinfo")] public SongsItem? RecentSonginfo { get; set; }
}
