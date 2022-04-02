namespace ArcaeaUnlimitedAPI.Beans;

internal class ArcaeaSong : List<ArcaeaCharts>, IEquatable<ArcaeaSong>
{
    internal string SongID => this[0].SongID;

    internal object ToJson() => new { song_id = SongID, difficulties = this };

    public new void Sort() { Sort((chart, another) => chart.RatingClass - another.RatingClass); }

    public bool Equals(ArcaeaSong? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return SongID.Equals(other.SongID);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ArcaeaSong)obj);
    }

    public override int GetHashCode() => SongID.GetHashCode();
}