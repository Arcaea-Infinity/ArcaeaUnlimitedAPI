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
    [HttpGet("/botarcapi/song/alias")]
    public object GetSongAlias([BindNever] ArcaeaSong song)
    {
        ArcaeaCharts.Aliases.TryGetValue(song.SongID, out List<string>? alias);
        return Success(alias ?? new());
    }
}
