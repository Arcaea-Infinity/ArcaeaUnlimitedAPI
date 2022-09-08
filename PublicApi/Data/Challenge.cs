﻿using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    private static readonly string[] PathList =
    {
        "auth/login", "user", "user/me", "friend/me/delete", "friend/me/add", "score/song/friend",
        "purchase/me/stamina/fragment", "world/map/me"
    };

    [UpdateCheck]
    [Auth]
    [HttpGet("/botarcapi/challenge")]
    [HttpGet("/botarcapi/data/challenge")]
    public object GetChallenge([FromQuery] ChallengeData? data)
    {
        if (data is null || PathList.All(i => !data.path.StartsWith(i))) return NotFound(null);
        return Success(ArcaeaFetch.GenerateChallenge("", data.body, data.path, data.time));
    }

    [UpdateCheck]
    [Auth]
    [HttpPost("/botarcapi/challenge")]
    [HttpPost("/botarcapi/data/challenge")]
    public object PostChallenges([FromBody] ChallengeData[] data)
    {
        if (data.Select(i => i.path).Any(path => PathList.All(i => !path.StartsWith(i)))) return NotFound(null);

        return Success(data.Select(i => ArcaeaFetch.GenerateChallenge("", i.body, i.path, i.time)));
    }

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming
    public record ChallengeData(string path = "", string body = "", ulong time = 0);
}
