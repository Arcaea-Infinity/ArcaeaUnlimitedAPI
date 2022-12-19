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
    [AuthorizationCheck(Order = 0)]
    [SongInfoConverter(Order = 1)]
    [DifficultyConverter(Order = 2)]
    [ChartConverter(Order = 3)]
    [HttpGet("assets/preview")]
    public object GetPreviewAssets([BindNever] ArcaeaCharts chart)
    {
        FileInfo fileinfo = new($"{GlobalConfig.Config.DataPath}/source/preview/{chart.SongID}/{chart.RatingClass}.jpeg");
        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/jpeg");
    }
}
