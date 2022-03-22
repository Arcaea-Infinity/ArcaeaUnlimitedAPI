using ArcaeaUnlimitedAPI.Beans;
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
}
