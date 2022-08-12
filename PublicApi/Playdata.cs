using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.PublicApi.Params;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/playdata")]
    public object GetPlaydata([FromQuery] SongInfoParams songInfo, [FromQuery] DifficultyParams difficultyInfo, int start, int end)
    {
        // validate request arguments

        var difficultyNum = difficultyInfo.Validate(out var difficultyerror);
        if (difficultyerror is not null) return difficultyerror;

        var song = songInfo.Validate(out var songerror);
        if (song is null) return songerror ?? Error.InvalidSongNameorID;

        // validate exist chart 
        if (ChartMissingCheck(song, difficultyNum)) return Error.NoThisLevel;

        return Success(PlayData.Query(start, end, song.SongID, difficultyNum));
    }
}
