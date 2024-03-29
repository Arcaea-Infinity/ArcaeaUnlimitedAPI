﻿namespace ArcaeaUnlimitedAPI.Beans;

public sealed class ArcaeaSong : List<ArcaeaCharts>, IEquatable<ArcaeaSong>
{
    internal string SongID => this[0].SongID;

    public bool Equals(ArcaeaSong? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return SongID.Equals(other.SongID);
    }

    internal object ToJson(bool usejsonlist = true)
    {
        if (usejsonlist && ArcaeaCharts.SongJsons.TryGetValue(SongID, out var json)) return json;

        var obj = new
                  {
                      song_id = SongID, difficulties = this, alias = ArcaeaCharts.Aliases.TryGetValue(SongID, out List<string>? alias) ? alias : new()
                  };
        ArcaeaCharts.SongJsons.TryAdd(SongID, obj);
        return obj;
    }

    internal new void Sort() => Sort((chart, another) => chart.RatingClass - another.RatingClass);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ArcaeaSong)obj);
    }

    public override int GetHashCode() => SongID.GetHashCode();
}
