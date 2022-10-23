using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [EnableCors]
    [SongInfoConverter(Order = 0)]
    [DifficultyConverter(Order = 1)]
    [ChartConverter(Order = 2)]
    [HttpGet("/botarcapi/assets/preview")]
    public object GetPreviewAssets([BindNever] ArcaeaCharts chart)
    {
        FileInfo fileinfo = new($"{GlobalConfig.Config.DataPath}/source/preview/{chart.SongID}/{chart.RatingClass}.jpeg");
        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/jpeg");
    }
}
