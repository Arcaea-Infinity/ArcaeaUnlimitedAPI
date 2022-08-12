using ArcaeaUnlimitedAPI.Beans;

namespace ArcaeaUnlimitedAPI.PublicApi.Params;

public record SongInfoParams(string? SongName, string? SongID) : IParams<ArcaeaSong>
{
    public ArcaeaSong? Validate(out Response? error)
    {
        error = null;

        if (!string.IsNullOrWhiteSpace(SongID)) return ArcaeaCharts.QueryById(SongID);

        if (string.IsNullOrWhiteSpace(SongName))
        {
            error = Response.Error.InvalidSongNameorID;
            return null;
        }

        var ls = ArcaeaCharts.Query(SongName);

        if (ls is null || ls.Count < 1)
        {
            error = Response.Error.SongNotFound;
            return null;
        }

        if (ls.Count > 1)
        {
            error = Response.Error.TooManySongs(ls);
            return null;
        }

        return ls[0];
    }
}
