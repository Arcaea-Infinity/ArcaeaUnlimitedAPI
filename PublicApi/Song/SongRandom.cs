using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/song/random")]
    public object GetSongRandom(int? start, int? end, bool withsonginfo = false)
    {
        if (!UserAgentCheck()) return NotFound(null);

        if (start is null && end is null)
        {
            var rsong = ArcaeaCharts.RandomSong();
            if (withsonginfo) return Success(new { id = rsong[0].SongID, songinfo = rsong });
            return Success(new { id = rsong[0].SongID });
        }

        if (start < 0 || end < 0) return Error.InvalidRange;
        if (start > end) return Error.InvalidEnd;

        var song = ArcaeaCharts.RandomSong(start, end);

        if (song is null) return Error.InvalidRange;

        if (withsonginfo) return Success(new { id = song.SongID, ratingClass = song.RatingClass, songinfo = song });
        return Success(new { id = song.SongID, ratingClass = song.RatingClass });
    }
}
