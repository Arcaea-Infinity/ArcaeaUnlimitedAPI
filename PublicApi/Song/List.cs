using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/song/list")]
    public object GetSongList() => Success(new { songs = ArcaeaCharts.SongJsons.Values });
}
