﻿using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

#pragma warning disable CS8618

public sealed class UserMeValue
{
    [JsonProperty("max_friend")]
    public int MaxFriend { get; set; }

    [JsonProperty("user_id")]
    public int UserID { get; set; }

    [JsonProperty("user_code")]
    public string UserCode { get; set; }

    [JsonProperty("friends")]
    public List<FriendsItem> Friends { get; set; }
}

public sealed class RegisterValue
{
    [JsonProperty("user_id")]
    public int UserID { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
}

public sealed class ArcUpdateValue
{
    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
}

public sealed class AddFriendValue
{
    [JsonProperty("friends")]
    public List<FriendsItem> Friends { get; set; }
}
