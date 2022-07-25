using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/assets/song")]
    public object GetSongAssets(string? songname, string? songid, string? difficulty, string? file)
    {
        FileInfo fileinfo;
        if (file is null)
        {
            if (!DifficultyInfo.TryParse(difficulty, out var difficultyNum)) difficultyNum = 2;

            var song = QuerySongInfo(songname, songid, out var songerror);

            if (song is null) return NotFound(songerror ?? Error.InvalidSongNameorID);

            if (difficultyNum == 3 && song.Count < 4) return Error.NoThisLevel;

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
