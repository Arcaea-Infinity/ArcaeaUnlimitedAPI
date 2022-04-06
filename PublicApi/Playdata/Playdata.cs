using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/playdata")]
    public object GetPlaydata(string? songname,string? songid, int difficulty, int start, int end)
    {
        var song = QuerySongInfoExperimental(songname, songid, out var songerror);
        if (song is null) return songerror ?? Error.InvalidSongNameorID;
        return Success(PlayData.Query(start, end, song.SongID, difficulty));
    }
}
