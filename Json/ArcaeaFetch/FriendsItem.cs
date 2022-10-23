using ArcaeaUnlimitedAPI.Beans;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

#pragma warning disable CS8618

public sealed class FriendsItem
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("user_id")]
    public int UserID { get; set; }

    [JsonProperty("is_mutual")]
    public bool IsMutual { get; set; }

    [JsonProperty("is_char_uncapped_override")]
    public bool IsCharUncappedOverride { get; set; }

    [JsonProperty("is_char_uncapped")]
    public bool IsCharUncapped { get; set; }

    [JsonProperty("is_skill_sealed")]
    public bool IsSkillSealed { get; set; }

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("join_date")]
    public long JoinDate { get; set; }

    [JsonProperty("character")]
    public int Character { get; set; }

    [JsonProperty("recent_score")]
    public List<Records>? RecentScore { get; set; }
}
