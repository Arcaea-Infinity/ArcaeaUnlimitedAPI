using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/song/info")]
    public object GetSongInfo(string? songname, string? songid)
    {
        if (!UserAgentCheck()) return NotFound(null);

        var song = QuerySongInfo(songname, songid, out var songerror);

        if (song is null) return songerror ?? Error.InvalidSongNameorID;

        return Success(song.ToJson());
    }
}
