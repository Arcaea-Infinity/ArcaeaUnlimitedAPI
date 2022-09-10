using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    private static readonly string[] PathList =
    {
        "auth/login", "user", "user/me", "friend/me/delete", "friend/me/add", "score/song/friend", "purchase/me/stamina/fragment", "world/map/me"
    };

    [AuthorizationCheck]
    [HttpGet("/botarcapi/challenge")]
    [HttpGet("/botarcapi/data/challenge")]
    public object GetChallenge([FromQuery] ChallengeData? data)
    {
        if (data is null || PathList.All(i => !data.Path.StartsWith(i))) return NotFound(null);
        return Success(ArcaeaFetch.GenerateChallenge(string.Empty, data.Body, data.Path, data.Time));
    }

    [AuthorizationCheck]
    [HttpPost("/botarcapi/challenge")]
    [HttpPost("/botarcapi/data/challenge")]
    public object PostChallenges([FromBody] ChallengeData[] data)
    {
        if (data.Select(i => i.Path).Any(path => PathList.All(i => !path.StartsWith(i)))) return NotFound(null);

        return Success(data.Select(i => ArcaeaFetch.GenerateChallenge(string.Empty, i.Body, i.Path, i.Time)));
    }

    public record ChallengeData(string Path = "", string Body = "", ulong Time = 0);
}
