using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/song/alias")]
    public object GetSongAlias(string? songname, string? songid)
    {
        if (!UserAgentCheck()) return NotFound(null);

        var song = QuerySongInfo(songname, songid, out var songerror);

        if (song is null) return songerror ?? Error.InvalidSongNameorID;

        return Success(ArcaeaSongs.GetAlias(song));
    }
}
