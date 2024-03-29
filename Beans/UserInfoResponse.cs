using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

public sealed class UserInfoResponse
{
    [JsonProperty("account_info")]
    public FriendsItem AccountInfo { get; set; }

    [JsonProperty("recent_score")]
    public List<Records>? RecentScore { get; set; }

    [JsonProperty("songinfo")]
    public IEnumerable<ArcaeaCharts>? Songinfo { get; set; }
}
