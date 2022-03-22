using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/assets/song")]
    public object GetSongAssets(string? songname, string? songid, string? difficulty, string? file)
    {
        if (!UserAgentCheck()) return NotFound(null);
        FileInfo fileinfo;
        if (file is null)
        {
            if (!DifficultyInfo.TryParse(difficulty, out var difficultyNum)) difficultyNum = 2;

            var song = QuerySongInfo(songname, songid, out var songerror);

            if (song is null) return NotFound(songerror ?? Error.InvalidSongNameorID);

            var difextend = difficultyNum switch
                            {
                                0 when song.JacketOverridePst == "true" => "_0",
                                1 when song.JacketOverridePrs == "true" => "_1",
                                3 when song.JacketOverrideByn == "true" => "_3",
                                _                                       => "",
                            };

            fileinfo = new($"{Config.DataRootPath}/sourse/songs/{song.SongId}{difextend}.jpg");
        }
        else
            fileinfo = new($"{Config.DataRootPath}/sourse/songs/{file}.jpg");

        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/jpeg");
    }

    [HttpGet("/botarcapi/assets/icon")]
    public object GetIconAssets(string? partner, bool awakened = false)
    {
        if (!UserAgentCheck()) return NotFound(null);

        // check for request arguments
        if (!int.TryParse(partner, out _)) return NotFound(Error.InvalidPartner);

        var fileinfo = new FileInfo($"{Config.DataRootPath}/sourse/char/{partner}{(awakened ? "u" : "")}_icon.png");

        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/png");
    }

    [HttpGet("/botarcapi/assets/char")]
    public object GetCharAssets(string? partner, bool awakened = false)
    {
        if (!UserAgentCheck()) return NotFound(null);

        // check for request arguments
        if (!int.TryParse(partner, out _)) return NotFound(Error.InvalidPartner);

        var fileinfo = new FileInfo($"{Config.DataRootPath}/sourse/char/{partner}{(awakened ? "u" : "")}.png");

        if (!fileinfo.Exists) return NotFound(Error.FileUnavailable);

        return PhysicalFile(fileinfo.FullName, "image/png");
    }
}
