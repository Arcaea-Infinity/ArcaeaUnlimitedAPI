using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.PublicApi.Params;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/song/alias")]
    public object GetSongAlias([FromQuery] SongInfoParams songInfo)
    {
        var song = songInfo.Validate(out var songerror);
        if (song is null) return songerror ?? Error.InvalidSongNameorID;

        return Success(ArcaeaCharts.Aliases[song.SongID]);
    }
}
