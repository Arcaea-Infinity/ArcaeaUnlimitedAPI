using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/song/random")]
    public object GetSongRandom(string? start, string? end, bool withsonginfo = false)
    {
        if (!UserAgentCheck()) return NotFound(null);
        if (start is null && end is null)
        {
            var rsong = ArcaeaSongs.RandomSong();
            if (withsonginfo) return Success(new { id = rsong.SongId, songinfo = rsong.ToJson() });
            return Success(new { id = rsong.SongId });
        }

        var (lower, upper) = RangeConverter(start, end);
        if (lower < 0 || upper < 0) return Error.InvalidRange;
        if (lower > upper) return Error.InvalidEnd;

        var (song, dif) = ArcaeaSongs.RandomSong(lower, upper);

        if (song is null) return Error.InvalidRange;

        if (withsonginfo) return Success(new { id = song.SongId, ratingClass = dif, songinfo = song.ToJson() });
        return Success(new { id = song.SongId, ratingClass = dif });
    }
}
