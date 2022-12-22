using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [EnableCors]
    [FileConverter(Order = 0)]
    [SongInfoConverter(Order = 1)]
    [DifficultyConverter(Order = 2)]
    [ChartConverter(Order = 3)]
    [HttpGet("assets/song")]
    public ActionResult GetSongAssets([BindNever] ArcaeaCharts chart)
    {
        var difextend = chart.JacketOverride ? $"_{chart.RatingClass}" : string.Empty;

        var fileinfo = new FileInfo($"{Config.DataPath}/source/songs/{chart.SongID}{difextend}.jpg");

        if (!fileinfo.Exists) return Error.FileUnavailable;

        return PhysicalFile(fileinfo.FullName, "image/jpeg");
    }

    [EnableCors]
    [HttpGet("assets/icon")]
    public ActionResult GetIconAssets(string? partner, bool awakened = false)
    {
        // check for request arguments
        if (!int.TryParse(partner, out _)) return Error.InvalidPartner;

        var fileinfo = new FileInfo($"{Config.DataPath}/source/char/{partner}{(awakened ? "u" : string.Empty)}_icon.png");

        if (!fileinfo.Exists) return Error.FileUnavailable;

        return PhysicalFile(fileinfo.FullName, "image/png");
    }

    [EnableCors]
    [HttpGet("assets/char")]
    public ActionResult GetCharAssets(string? partner, bool awakened = false)
    {
        // check for request arguments
        if (!int.TryParse(partner, out _)) return Error.InvalidPartner;

        var fileinfo = new FileInfo($"{Config.DataPath}/source/char/{partner}{(awakened ? "u" : string.Empty)}.png");

        if (!fileinfo.Exists) return Error.FileUnavailable;

        return PhysicalFile(fileinfo.FullName, "image/png");
    }
}
