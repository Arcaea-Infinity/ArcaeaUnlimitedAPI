using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/song/info")]
    public object GetSongInfo(string? songname, string? songid)
    {
        var song = QuerySongInfo(songname, songid, out var songerror);
        if (song is null) return songerror ?? Error.InvalidSongNameorID;
        return Success(song.ToJson());
    }
}
