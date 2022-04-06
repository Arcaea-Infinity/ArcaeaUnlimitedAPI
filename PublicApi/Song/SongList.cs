using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/song/list")]
    public object GetSongList()
    {
        if (!UserAgentCheck()) return NotFound(null);
        return Success(new { songs = ArcaeaSongs.SongJsonList.Value.Values.ToArray() });
    }

    [EnableCors]
    [HttpGet("/botarcapi/test/song/list")]
    public object GetSongListExperimental() => Success(new { songs = ArcaeaCharts.SongJsons.Values });
}
