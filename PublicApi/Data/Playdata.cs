using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors] 
    [SongInfoConverter]
    [DifficultyConverter]
    [HttpGet("/botarcapi/playdata")]
    [HttpGet("/botarcapi/data/playdata")]
    public object GetPlaydata([BindNever] ArcaeaSong song, [BindNever] sbyte difficulty, int start, int end)
    {
        // validate exist chart 
        if (ChartMissingCheck(song, difficulty)) return Error.NoThisLevel;

        return Success(PlayData.Query(start, end, song.SongID, difficulty));
    }
}
