using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [FileConverter(Order = 0)]
    [SongInfoConverter(Order = 1)]
    [DifficultyConverter(Order = 2)]
    [HttpGet("/botarcapi/assets/song")]
    public object GetSongAssets([BindNever] ArcaeaSong song, [BindNever] sbyte difficulty)
    {
        // validate exist chart 
        if (ChartMissingCheck(song, difficulty)) return Error.NoThisLevel;

        var difextend = song[difficulty].JacketOverride
            ? $"_{difficulty}"
            : "";

        var fileinfo = new FileInfo($"{Config.DataPath}/source/songs/{song.SongID}{difextend}.jpg");


        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/jpeg");
    }

    [EnableCors]
    [HttpGet("/botarcapi/assets/icon")]
    public object GetIconAssets(string? partner, bool awakened = false)
    {
        // check for request arguments
        if (!int.TryParse(partner, out _)) return NotFound(Error.InvalidPartner);

        var fileinfo = new FileInfo($"{Config.DataPath}/source/char/{partner}{(awakened ? "u" : "")}_icon.png");

        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/png");
    }

    [EnableCors]
    [HttpGet("/botarcapi/assets/char")]
    public object GetCharAssets(string? partner, bool awakened = false)
    {
        // check for request arguments
        if (!int.TryParse(partner, out _)) return NotFound(Error.InvalidPartner);

        var fileinfo = new FileInfo($"{Config.DataPath}/source/char/{partner}{(awakened ? "u" : "")}.png");

        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/png");
    }
}
