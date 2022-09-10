using System.Collections.Concurrent;
using System.Text;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.Songlist;
using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

public partial class ArcaeaCharts
{
    internal static readonly ConcurrentDictionary<string, ArcaeaSong> Songs;
    internal static readonly ConcurrentDictionary<string, object> SongJsons;
    internal static readonly ConcurrentDictionary<string, List<string>> Aliases;

    internal static ArcaeaCharts[] SortedCharts => SortByRating.ToArray();

    internal static ArcaeaCharts? QueryByRecord(Records? record)
    {
        if (record is null) return null;
        return GetById(record.SongID)![record.Difficulty];
    }

    internal static ArcaeaSong? QueryById(string? songid) => GetById(songid);

    internal static List<ArcaeaSong>? Query(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias)) return default;

        if (AliasCache.ContainsKey(alias)) return AliasCache[alias];

        var data = GetById(alias) ?? GetByName(Songs, alias) ?? GetByAlias(alias);

        if (data != null) return new() { data };

        var abbrdata = new List<ArcaeaSong>();

        Abbreviations.ForAllItems<ArcaeaSong, string, List<string>>((song, value) =>
                                                                    {
                                                                        if (Utils.StringCompareHelper.Equals(value, alias) &&
                                                                            !abbrdata.Contains(song))
                                                                            abbrdata.Add(song);
                                                                    });

        if (abbrdata.Count > 0) return abbrdata;

        return GetByPriorityQueue(alias);
    }

    internal static void Insert(SongItem item)
    {
        for (var i = GetById(item.Id)?.Count ?? 0; i < item.Difficulties.Count; i++)
        {
            var chart = new ArcaeaCharts
                        {
                            RatingClass = i,
                            SongID = item.Id,
                            NameEn = item.Difficulties[i].TitleLocalized?.En ?? item.TitleLocalized.En,
                            NameJp = item.Difficulties[i].TitleLocalized?.Ja ?? item.TitleLocalized.Ja ?? string.Empty,
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
                            Difficulty = item.Difficulties[i].Rating * 2 + (item.Difficulties[i].RatingPlus == true ? 1 : 0)
                        };

            Songs.TryAddOrInsert(item.Id, chart);

            DatabaseManager.Song.Value.Insert(chart);
        }
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
            Log.RatingLog(record.SongID, chart.NameEn, record.Difficulty, chart.Rating, @const);
            chart.Rating = @const;
            chart.Note = record.MissCount + record.NearCount + record.PerfectCount;
            var str = "UPDATE `charts` SET rating = ?, note = ? WHERE song_id = ? AND rating_class = ?;";
            DatabaseManager.Song.Value.Execute(str, chart.Rating, chart.Note, chart.SongID, chart.RatingClass);
            Sort();
        }
    }

    internal static ArcaeaSong RandomSong() => Utils.RandomHelper.GetRandomItem(Songs.Values.ToArray())!;

    internal static ArcaeaCharts? RandomSong(int? start, int? end)
        => Utils.RandomHelper.GetRandomItem(GetByDifficulty(start ?? 0, end ?? 24).ToArray());
}

public partial class ArcaeaCharts
{
    [NonSerialized]
    private static readonly List<ArcaeaCharts> SortByRating;

    [NonSerialized]
    private static readonly ConcurrentDictionary<ArcaeaSong, List<string>> Abbreviations = new();

    [NonSerialized]
    private static readonly ConcurrentDictionary<ArcaeaSong, List<string>> Names = new();

    [NonSerialized]
    private static readonly ConcurrentDictionary<string, List<ArcaeaSong>> AliasCache = new();

    static ArcaeaCharts()
    {
        Songs = new();
        SongJsons = new();
        Aliases = new();
        SortByRating = new();

        foreach (var alias in DatabaseManager.Song.SelectAll<ArcaeaAlias>()) Aliases.TryAddOrInsert(alias.SongID, alias.Alias);

        foreach (var chart in DatabaseManager.Song.SelectAll<ArcaeaCharts>())
        {
            chart.Init();
            Songs.TryAddOrInsert(chart.SongID, chart);
        }

        foreach (var (sid, value) in Songs)
        {
            value.Sort();

            var abbrs = new List<string>();
            var names = new List<string>();

            for (var index = 0; index < value.Count; index++)
            {
                if (index == 0 || value[index].AudioOverride)
                {
                    abbrs.Add(GetAbbreviation(value[index].NameEn));
                    names.Add(value[index].NameEn);
                    if (!string.IsNullOrWhiteSpace(value[index].NameJp))
                    {
                        abbrs.Add(GetAbbreviation(value[index].NameJp));
                        names.Add(value[index].NameJp);
                    }
                }
            }

            Abbreviations.TryAdd(value, abbrs);
            Names.TryAdd(value, names);
            SongJsons.TryAdd(sid, value.ToJson(false));
        }

        Sort();
    }

    private void Init() => Package = PackageInfo.GetById(Set)?.Name;

    private static string GetAbbreviation(string str)
    {
        var sb = new StringBuilder();
        sb.Append(str[0]);

        for (var index = 0; index < str.Length - 1; ++index)
        {
            if (str[index] == ' ') sb.Append(str[index + 1]);
        }

        return sb.ToString();
    }

    private static void Sort()
    {
        lock (SortByRating)
        {
            SortByRating.Clear();

            Songs.ForAllItems<string, ArcaeaCharts, ArcaeaSong>((_, value) => SortByRating.Add(value));

            SortByRating.Sort((tuple, valueTuple) => valueTuple.Rating - tuple.Rating);
        }
    }
}

public partial class ArcaeaCharts
{
    private static ArcaeaSong? GetById(string? songid) => songid is not null && Songs.TryGetValue(songid, out var value) ? value : null;

    private static ArcaeaSong? GetByName(ConcurrentDictionary<string, ArcaeaSong> values, string alias)
    {
        values.TryTakeValues((ArcaeaCharts item) => Utils.StringCompareHelper.Equals(item.NameEn, alias) || Utils.StringCompareHelper.Equals(item.NameJp, alias),
                             out var result);

        return result;
    }

    private static ArcaeaSong? GetByAlias(string alias)
    {
        Aliases.TryTakeKey<string, string, List<string>>(value => Utils.StringCompareHelper.Equals(value, alias), out var result);
        return GetById(result);
    }

    private static List<ArcaeaSong>? GetByPriorityQueue(string alias)
    {
        var dic = new PriorityQueue<ArcaeaSong, byte>();

        Aliases.ForAllItems<string, string, List<string>>((song, sid) => Enqueue(dic, alias, sid, GetById(song)!, 1, 4));

        dic.TryPeek(out _, out var firstpriority);

        if (firstpriority != 1)
            foreach (var (sid, song) in Songs)
                Enqueue(dic, alias, sid, song, 2, 5);

        dic.TryPeek(out _, out firstpriority);

        if (firstpriority != 2) Names.ForAllItems<ArcaeaSong, string, List<string>>((song, name) => Enqueue(dic, alias, name, song, 3, 6));

        if (dic.Count == 0) return default;

        dic.TryDequeue(out var firstobj, out var lowestpriority);

        var ls = new List<ArcaeaSong> { firstobj! };

        while (dic.TryDequeue(out var obj, out var priority) && priority == lowestpriority)
        {
            if (!ls.Contains(obj)) ls.Add(obj);
        }

        AliasCache.TryAdd(alias, ls);
        return ls;
    }

    private static IEnumerable<ArcaeaCharts> GetByDifficulty(int lowerlimit, int upperlimit)
        => Songs.Values.SelectMany(charts => charts).Where(t => t.Difficulty >= lowerlimit && t.Difficulty <= upperlimit);

    private static void Enqueue(
        PriorityQueue<ArcaeaSong, byte> dic,
        string alias,
        string key,
        ArcaeaSong song,
        byte upperpriority,
        byte lowerpriority)
    {
        if (Utils.StringCompareHelper.Contains(key, alias)) dic.Enqueue(song, upperpriority);
        if (Utils.StringCompareHelper.Contains(alias, key)) dic.Enqueue(song, lowerpriority);
    }

    private static double CalcSongConst(int score, double rating)
        => score switch
           {
               >= 10000000 => rating - 2,
               >= 9800000  => rating - 1 - (double)(score - 9800000) / 200000,
               _           => rating > 0 ? rating - (double)(score - 9500000) / 300000 : 0
           };
}

[Serializable]
[Table("charts")]
[DatabaseManager.CreateTableSqlAttribute(
                                            "CREATE TABLE `charts`(`song_id` TEXT PRIMARY KEY NOT NULL DEFAULT '', `rating_class` INTEGER NOT NULL DEFAULT 0, `name_en` TEXT NOT NULL DEFAULT '', `name_jp` TEXT DEFAULT '', `artist` TEXT NOT NULL DEFAULT '', `bpm` TEXT NOT NULL DEFAULT '', `bpm_base` DOUBLE NOT NULL DEFAULT 0, `set` TEXT NOT NULL DEFAULT '', `time` INTEGER DEFAULT 0, `side` INTEGER NOT NULL DEFAULT 0, `world_unlock` BOOLEAN NOT NULL DEFAULT 0, `remote_download` BOOLEAN DEFAULT '', `bg` TEXT NOT NULL DEFAULT '', `date` INTEGER NOT NULL DEFAULT 0, `version` TEXT NOT NULL DEFAULT '', `difficulty` INTEGER NOT NULL DEFAULT 0, `rating` INTEGER NOT NULL DEFAULT 0, `note` INTEGER NOT NULL DEFAULT 0, `chart_designer` TEXT DEFAULT '', `jacket_designer` TEXT DEFAULT '', `jacket_override` BOOLEAN NOT NULL DEFAULT 0, `audio_override` BOOLEAN NOT NULL DEFAULT 0);")]
public partial class ArcaeaCharts
{
#region DataProperties

    [JsonIgnore] [PrimaryKey] [Column("song_id")]
    public string SongID { get; set; } = string.Empty;

    [JsonIgnore] [PrimaryKey] [Column("rating_class")]
    public int RatingClass { get; set; }

    [JsonProperty("name_en")] [Column("name_en")]
    public string NameEn { get; set; } = string.Empty;

    [JsonProperty("name_jp")] [Column("name_jp")]
    public string NameJp { get; set; } = string.Empty;

    [JsonProperty("artist")] [Column("artist")]
    public string Artist { get; set; } = string.Empty;

    [JsonProperty("bpm")] [Column("bpm")]
    public string Bpm { get; set; } = string.Empty;

    [JsonProperty("bpm_base")] [Column("bpm_base")]
    public double BpmBase { get; set; }

    [JsonProperty("set")] [Column("set")]
    public string Set { get; set; } = string.Empty;

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
    public string Background { get; set; } = string.Empty;

    [JsonProperty("date")] [Column("date")]
    public long Date { get; set; }

    [JsonProperty("version")] [Column("version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty("difficulty")] [Column("difficulty")]
    public int Difficulty { get; set; }

    [JsonProperty("rating")] [Column("rating")]
    public int Rating { get; set; }

    [JsonProperty("note")] [Column("note")]
    public int Note { get; set; }

    [JsonProperty("chart_designer")] [Column("chart_designer")]
    public string ChartDesigner { get; set; } = string.Empty;

    [JsonProperty("jacket_designer")] [Column("jacket_designer")]
    public string JacketDesigner { get; set; } = string.Empty;

    [JsonProperty("jacket_override")] [Column("jacket_override")]
    public bool JacketOverride { get; set; }

    [JsonProperty("audio_override")] [Column("audio_override")]
    public bool AudioOverride { get; set; }

#endregion
}
