using ArcaeaUnlimitedAPI.PublicApi.Params;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/assets/song")]
    public object GetSongAssets([FromQuery] SongInfoParams songInfo,[FromQuery] DifficultyParams difficultyInfo, string? file)
    {
        FileInfo fileinfo;
        if (file is null)
        {
            var difficultyNum = difficultyInfo.Validate(out _);
      
            var song = songInfo.Validate(out var songerror);
            if (song is null) return songerror ?? Error.InvalidSongNameorID;

            // validate exist chart 
            if (ChartMissingCheck(song, difficultyNum)) return Error.NoThisLevel;
          
            var difextend = song[difficultyNum].JacketOverride
                ? $"_{difficultyNum}"
                : "";

            fileinfo = new($"{Config.DataPath}/source/songs/{song.SongID}{difextend}.jpg");
        }
        else
        {
            if (file.Contains("/")) return NotFound(Error.FileUnavailable);
            fileinfo = new($"{Config.DataPath}/source/songs/{file}.jpg");
        }

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
