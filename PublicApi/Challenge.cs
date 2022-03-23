using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    private static readonly string[] PathList = new[]
                                                {
                                                    "auth/login", "user", "user/me", "friend/me/delete",
                                                    "friend/me/add", "score/song/friend",
                                                    "purchase/me/stamina/fragment", "world/map/me"
                                                };

    [HttpGet("/botarcapi/challenge")]
    public object GetChallenge(string path, string? body, ulong time = 0)
    {
        if (!UserAgentCheck() || PathList.All(i => !path.StartsWith(i))) return NotFound(null);
        if (NeedUpdate) return Error.NeedUpdate;
        return Success(ArcaeaFetch.GenerateChallenge("", body ?? "", path, time));
    }

    [HttpPost("/botarcapi/challenge")]
    public object PostChallenges([FromBody] ChallengeData[] data)
    {
        if (!UserAgentCheck()) return NotFound(null);
        if (NeedUpdate) return Error.NeedUpdate;
        if (data.Select(i => i.path).Any(path => PathList.All(i => !path.StartsWith(i)))) return NotFound(null);

        return Success(data.Select(i => ArcaeaFetch.GenerateChallenge("", i.body ?? "", i.path, i.time)));
    }

    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming
    public record ChallengeData(string path, string? body, ulong time = 0);
}
