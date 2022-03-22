using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.Json.Songlist;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

public class UserInfoResponse
{
    [JsonProperty("account_info")] public FriendsItem AccountInfo { get; set; }
    
    [JsonProperty("recent_score")] public List<Records>? RecentScore { get; set; }

    [JsonProperty("songinfo")] public IEnumerable<SongsItem>? Songinfo { get; set; }
}
