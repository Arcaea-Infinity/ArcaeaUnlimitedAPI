using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.Songlist;
using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

[Serializable]
[Table("charts")]
[DatabaseManager.CreateTableSqlAttribute(
                                            "CREATE TABLE `charts`(`song_id` TEXT PRIMARY KEY NOT NULL DEFAULT '', `rating_class` INTEGER NOT NULL DEFAULT 0, `name_en` TEXT NOT NULL DEFAULT '', `name_jp` TEXT DEFAULT '', `artist` TEXT NOT NULL DEFAULT '', `bpm` TEXT NOT NULL DEFAULT '', `bpm_base` DOUBLE NOT NULL DEFAULT 0, `set` TEXT NOT NULL DEFAULT '', `time` INTEGER DEFAULT 0, `side` INTEGER NOT NULL DEFAULT 0, `world_unlock` BOOLEAN NOT NULL DEFAULT 0, `remote_download` BOOLEAN DEFAULT '', `bg` TEXT NOT NULL DEFAULT '', `date` INTEGER NOT NULL DEFAULT 0, `version` TEXT NOT NULL DEFAULT '', `difficulty` INTEGER NOT NULL DEFAULT 0, `rating` INTEGER NOT NULL DEFAULT 0, `note` INTEGER NOT NULL DEFAULT 0, `chart_designer` TEXT DEFAULT '', `jacket_designer` TEXT DEFAULT '', `jacket_override` BOOLEAN NOT NULL DEFAULT 0, `audio_override` BOOLEAN NOT NULL DEFAULT 0);")]
public class ArcaeaCharts
{
    [NonSerialized] internal static readonly List<(string sid, int dif, int rating)> SortByRating;

    [NonSerialized] private static readonly ConcurrentDictionary<string, ArcaeaSong> SongList;
    [NonSerialized] private static readonly ConcurrentDictionary<string, List<string>> AliasList;
    [NonSerialized] private static readonly ConcurrentDictionary<string, List<string>> Abbreviations = new();
    [NonSerialized] private static readonly ConcurrentDictionary<string, List<ArcaeaSong>> Aliascache = new();

    static ArcaeaCharts()
    {
        SongList = new();

        foreach (var chart in DatabaseManager.Song.SelectAll<ArcaeaCharts>())
        {
            chart.Init();
            if (SongList.ContainsKey(chart.SongID))
                SongList[chart.SongID].Add(chart);
            else
                SongList.TryAdd(chart.SongID, new(chart.SongID) { chart });
        }

        foreach (var (_, value) in SongList) value.Sort();

        AliasList = new();

        foreach (var alias in DatabaseManager.Song.SelectAll<ArcaeaAlias>())
            if (AliasList.ContainsKey(alias.SongId))
                AliasList[alias.SongId].Add(alias.Alias);
            else
                AliasList.TryAdd(alias.SongId, new() { alias.Alias });

        SortByRating = new();
        Sort();
    }

    public void Init()
    {
        Package = PackageInfo.GetById(Set)?.Name;

        if (Abbreviations.ContainsKey(SongID) && !AudioOverride) return;

        var ls = new List<string>() { GetAbbreviation(NameEn) };

        if (!string.IsNullOrEmpty(NameJp)) ls.Add(GetAbbreviation(NameJp));

        Abbreviations.TryAdd(SongID, ls);
    }

    internal static string GetAbbreviation(string str)
    {
        var sb = new StringBuilder();
        sb.Append(str[0]);

        for (var index = 0; index < str.Length - 1; ++index)
            if (str[index] == ' ')
                sb.Append(str[index + 1]);

        return sb.ToString();
    }

    internal static void Sort()
    {
        lock (SortByRating)
        {
            SortByRating.Clear();

            foreach (var (sid, item) in SongList)
                for (var i = 0; i < item.Count; i++)
                    SortByRating.Add((sid, i, item[i].Rating));

            SortByRating.Sort((tuple, valueTuple) => valueTuple.Item3 - tuple.Item3);
        }
    }

    internal static ArcaeaSong? GetById(string? songid) =>
        songid is not null && SongList.TryGetValue(songid, out var value)
            ? value
            : null;

    internal static IEnumerable<string> GetAlias(ArcaeaSong song) =>
        AliasList.TryGetValue(song.SongID, out var result)
            ? result
            : Array.Empty<string>();

    internal static string? GetByAlias(ConcurrentDictionary<string, List<string>> values, string alias)
    {
        foreach (var (key, value) in values)
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var item in value)
                if (Utils.StringCompareHelper.Equals(item, alias))
                    return key;

        return default;
    }

    internal static ArcaeaSong? GetByName(ConcurrentDictionary<string, ArcaeaSong> values, string alias)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var value in values.Values)
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in value)
                if (Utils.StringCompareHelper.Equals(item.NameEn, alias)
                    || Utils.StringCompareHelper.Equals(item.NameJp, alias))
                    return value;

        return default;
    }

    internal static List<ArcaeaSong> GetByAlias(string alias)
    {
        var empty = new List<ArcaeaSong>();

        if (string.IsNullOrWhiteSpace(alias)) return empty;

        if (Aliascache.ContainsKey(alias)) return Aliascache[alias];

        var data = GetById(alias) ?? GetByName(SongList, alias) ?? GetById(GetByAlias(AliasList, alias));

        if (data != null) return new() { data };

        var abbrdata = new List<ArcaeaSong>();

        foreach (var (key, value) in Abbreviations)
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var item in value)
                if (Utils.StringCompareHelper.Equals(item, alias))
                {
                    var obj = GetById(key)!;
                    if (!abbrdata.Contains(obj)) abbrdata.Add(GetById(key)!);
                }

        if (abbrdata.Count > 0) return abbrdata;

        var dic = new PriorityQueue<string, byte>();

        foreach (var (key, values) in AliasList)
        {
            foreach (var value in values)
            {
                if (Utils.StringCompareHelper.Contains(value, alias)) dic.Enqueue(key, 1);
                if (Utils.StringCompareHelper.Contains(alias, value)) dic.Enqueue(key, 4);
            }
        }

        dic.TryPeek(out _, out var firstpriority);

        if (firstpriority != 1)
        {
            foreach (var songdata in SongList.Values)
            {
                if (Utils.StringCompareHelper.Contains(songdata.SongID, alias)) dic.Enqueue(songdata.SongID, 2);
                if (Utils.StringCompareHelper.Contains(alias, songdata.SongID)) dic.Enqueue(songdata.SongID, 5);

                dic.TryPeek(out _, out firstpriority);

                if (firstpriority != 2)
                {
                    foreach (var chart in songdata)
                    {
                        if (Utils.StringCompareHelper.Contains(chart.NameEn, alias)) dic.Enqueue(songdata.SongID, 3);
                        if (Utils.StringCompareHelper.Contains(alias, chart.NameEn)) dic.Enqueue(songdata.SongID, 6);

                        if (!string.IsNullOrWhiteSpace(chart.NameJp))
                        {
                            if (Utils.StringCompareHelper.Contains(chart.NameJp, alias))
                                dic.Enqueue(songdata.SongID, 3);
                            if (Utils.StringCompareHelper.Contains(alias, chart.NameJp))
                                dic.Enqueue(songdata.SongID, 6);
                        }
                    }
                }
            }
        }

        if (dic.Count == 0) return empty;

        dic.TryDequeue(out var firstobj, out var lowestpriority);

        var ls = new List<ArcaeaSong> { GetById(firstobj)! };

        while (dic.TryDequeue(out var obj, out var priority) && priority == lowestpriority)
        {
            var song = GetById(obj)!;
            if (!ls.Contains(song)) ls.Add(song);
        }

        Aliascache.TryAdd(alias, ls);
        return ls;
    }

    internal static void Insert(SongItem item)
    {
        var ls = new ArcaeaSong(item.Id);

        for (var i = 0; i < item.Difficulties.Count; i++)
        {
            var chart = new ArcaeaCharts
                        {
                            RatingClass = i,
                            SongID = item.Id,
                            NameEn = item.Difficulties[i].TitleLocalized?.En ?? item.TitleLocalized.En,
                            NameJp = item.Difficulties[i].TitleLocalized?.Ja ?? item.TitleLocalized.Ja ?? "",
                            Bpm = item.Difficulties[i].Bpm ?? item.Bpm,
                            BpmBase = item.Difficulties[i].BpmBase ?? item.BpmBase,
                            Set = item.Difficulties[i].Set ?? item.Set,
                            Artist = item.Difficulties[i].Artist ?? item.Artist,
                            Side = item.Difficulties[i].Side ?? item.Side,
                            Date = item.Difficulties[i].Date ?? item.Date,
                            Version = item.Difficulties[i].Version ?? item.Version,
                            WorldUnlock = item.Difficulties[i].WorldUnlock ?? item.WorldUnlock,
                            RemoteDownload = item.Difficulties[i].NeedDownload ?? item.NeedDownload,
                            Background = item.Difficulties[i].Background ?? item.Background,
                            ChartDesigner = item.Difficulties[i].ChartDesigner,
                            JacketDesigner = item.Difficulties[i].JacketDesigner,
                            JacketOverride = item.Difficulties[i].JacketOverride,
                            AudioOverride = item.Difficulties[i].AudioOverride,
                            Difficulty = item.Difficulties[i].Rating * 2 + (item.Difficulties[i].RatingPlus == true
                                ? 1
                                : 0)
                        };

            chart.Init();
            ls.Add(chart);

            DatabaseManager.Song.Value.Insert(chart);
        }

        SongList.TryAdd(item.Id, ls);
    }

    internal static void UpdateRating(Records record)
    {
        var @const = (int)Math.Round(CalcSongConst(record.Score, record.Rating) * 10);
        if (@const == 0) return;

        var item = GetById(record.SongID);

        // not update yet
        if (item is null) return;

        var chart = item[record.Difficulty];
        if (chart.Rating != @const)
        {
            chart.Rating = @const;
            chart.Note = record.MissCount + record.NearCount + record.PerfectCount;

            DatabaseManager.Song.Update(chart);
            Sort();
        }
    }

    private static double CalcSongConst(int score, double rating)
    {
        return score switch
               {
                   >= 10000000 => rating - 2,
                   >= 9800000  => rating - 1 - (double)(score - 9800000) / 200000,
                   _ => rating > 0
                       ? rating - (double)(score - 9500000) / 300000
                       : 0
               };
    }

    internal static IEnumerable<ArcaeaCharts> GetByConstRange(int lowerlimit, int upperlimit) =>
        SongList.Values.SelectMany(charts => charts)
                .Where(t => t.Difficulty >= lowerlimit && t.Difficulty <= upperlimit);

    internal static ArcaeaSong RandomSong() => Utils.RandomHelper.GetRandomItem(SongList.Values.ToArray())!;

    internal static ArcaeaCharts? RandomSong(int? start, int? end) =>
        Utils.RandomHelper.GetRandomItem(GetByConstRange(start ?? 0, end ?? 24).ToArray());


#region DataProperties

    [JsonIgnore] [PrimaryKey] [Column("song_id")]
    public string SongID { get; set; } = "";

    [JsonIgnore] [PrimaryKey] [Column("rating_class")]
    public int RatingClass { get; set; }

    [JsonProperty("name_en")] [Column("name_en")]
    public string NameEn { get; set; } = "";

    [JsonProperty("name_jp")] [Column("name_jp")]
    public string NameJp { get; set; } = "";

    [JsonProperty("artist")] [Column("artist")]
    public string Artist { get; set; } = "";

    [JsonProperty("bpm")] [Column("bpm")]
    public string Bpm { get; set; } = "";

    [JsonProperty("bpm_base")] [Column("bpm_base")]
    public double BpmBase { get; set; }

    [JsonProperty("set")] [Column("set")]
    public string Set { get; set; } = "";

    [JsonProperty("set_friendly")] [Ignore]
    public string? Package { get; set; }

    [JsonProperty("time")] [Column("time")]
    public int Time { get; set; }

    [JsonProperty("side")] [Column("side")]
    public int Side { get; set; }

    [JsonProperty("world_unlock")] [Column("world_unlock")]
    public bool WorldUnlock { get; set; }

    [JsonProperty("remote_download")] [Column("remote_download")]
    public bool RemoteDownload { get; set; }

    [JsonProperty("bg")] [Column("bg")]
    public string Background { get; set; } = "";

    [JsonProperty("date")] [Column("date")]
    public long Date { get; set; }

    [JsonProperty("version")] [Column("version")]
    public string Version { get; set; } = "";

    [JsonProperty("difficulty")] [Column("difficulty")]
    public int Difficulty { get; set; }

    [JsonProperty("rating")] [Column("rating")]
    public int Rating { get; set; }

    [JsonProperty("note")] [Column("note")]
    public int Note { get; set; }

    [JsonProperty("chart_designer")] [Column("chart_designer")]
    public string ChartDesigner { get; set; } = "";

    [JsonProperty("jacket_designer")] [Column("jacket_designer")]
    public string JacketDesigner { get; set; } = "";

    [JsonProperty("jacket_override")] [Column("jacket_override")]
    public bool JacketOverride { get; set; }

    [JsonProperty("audio_override")] [Column("audio_override")]
    public bool AudioOverride { get; set; }

#endregion
}

internal class ArcaeaSong : IEnumerable<ArcaeaCharts>, IEquatable<ArcaeaSong>
{
    [JsonRequired]
    [JsonProperty("difficulties")]
    private readonly List<ArcaeaCharts> _charts;

    [JsonRequired]
    [JsonProperty("song_id")]
    internal readonly string SongID;

    internal int Count => _charts.Count;

    public ArcaeaSong(string songID)
    {
        _charts = new();
        SongID = songID;
    }

    public ArcaeaCharts this[int index] => _charts[index];

    public void Add(ArcaeaCharts chart) => _charts.Add(chart);
    public IEnumerator<ArcaeaCharts> GetEnumerator() => _charts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Sort() { _charts.Sort((chart, another) => chart.RatingClass - another.RatingClass); }

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
