namespace ArcaeaUnlimitedAPI.Beans;

public class ArcaeaSong : List<ArcaeaCharts>, IEquatable<ArcaeaSong>
{
    internal string SongID => this[0].SongID;

    public bool Equals(ArcaeaSong? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return SongID.Equals(other.SongID);
    }

    public object ToJson(bool usejsonlist = true)
    {
        if (usejsonlist && ArcaeaCharts.SongJsons.ContainsKey(SongID)) return ArcaeaCharts.SongJsons[SongID];

        var obj = new
                  {
                      song_id = SongID,
                      difficulties = this,
                      alias = ArcaeaCharts.Aliases.ContainsKey(SongID)
                          ? ArcaeaCharts.Aliases[SongID]
                          : new()
                  };
        ArcaeaCharts.SongJsons.TryAdd(SongID, obj);
        return obj;
    }

    public new void Sort() { Sort((chart, another) => chart.RatingClass - another.RatingClass); }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ArcaeaSong)obj);
    }

    public override int GetHashCode() => SongID.GetHashCode();
}
