using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [SongInfoConverter(Order = 0)]
    [DifficultyConverter(Order = 1)]
    [ChartConverter(Order = 2)]
    [HttpGet("/botarcapi/playdata")]
    [HttpGet("/botarcapi/data/playdata")]
    public object GetPlaydata([BindNever] ArcaeaCharts chart, int start, int end)
        => Success(PlayData.Query(start, end, chart.SongID, chart.RatingClass));

    [EnableCors]
    [SongInfoConverter(Order = 0)]
    [DifficultyConverter(Order = 1)]
    [ChartConverter(Order = 2)]
    [HttpGet("/botarcapi/data/density")]
    public object GetPlaydataArray([BindNever] ArcaeaCharts chart)
        => PlayDataArray.Query(chart).Select(i => new[] { i.FormattedScore, i.FormattedPotential, i.Count });
}
