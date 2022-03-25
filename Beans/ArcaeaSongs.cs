using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.Json.Songlist;
using ArcaeaUnlimitedAPI.PublicApi;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

[Serializable]
[Table("songs")]
[DatabaseManager.CreateTableSqlAttribute(
                                            "CREATE TABLE `songs`(`sid` TEXT PRIMARY KEY ASC NOT NULL, `name_en` TEXT NOT NULL DEFAULT '', `name_jp` TEXT NOT NULL DEFAULT '', `bpm` TEXT NOT NULL DEFAULT '', `bpm_base` INTEGER NOT NULL DEFAULT 0, `pakset` TEXT NOT NULL DEFAULT '', `artist` TEXT NOT NULL DEFAULT '', `time` INTEGER NOT NULL DEFAULT 0, `side` INTEGER NOT NULL DEFAULT 0, `date` INTEGER NOT NULL DEFAULT 0, `version` TEXT NOT NULL DEFAULT '', `world_unlock` TEXT NOT NULL DEFAULT 'false', `remote_download` TEXT NOT NULL DEFAULT 'false', `rating_pst` INTEGER NOT NULL DEFAULT 0, `rating_prs` INTEGER NOT NULL DEFAULT 0, `rating_ftr` INTEGER NOT NULL DEFAULT 0, `rating_byn` INTEGER NOT NULL DEFAULT (- 1), `notes_pst` INTEGER NOT NULL DEFAULT 0, `notes_prs` INTEGER NOT NULL DEFAULT 0, `notes_ftr` INTEGER NOT NULL DEFAULT 0, `notes_byn` INTEGER NOT NULL DEFAULT (- 1), `chart_designer_pst` TEXT NOT NULL DEFAULT '', `chart_designer_prs` TEXT NOT NULL DEFAULT '', `chart_designer_ftr` TEXT NOT NULL DEFAULT '', `chart_designer_byn` TEXT NOT NULL DEFAULT '', `jacket_designer_pst` TEXT NOT NULL DEFAULT '', `jacket_designer_prs` TEXT NOT NULL DEFAULT '', `jacket_designer_ftr` TEXT NOT NULL DEFAULT '', `jacket_designer_byn` TEXT NOT NULL DEFAULT '', `jacket_override_pst` TEXT NOT NULL DEFAULT 'false', `jacket_override_prs` TEXT NOT NULL DEFAULT 'false', `jacket_override_ftr` TEXT NOT NULL DEFAULT 'false', `jacket_override_byn` TEXT NOT NULL DEFAULT 'false', CHECK(`side` IN (0, 1)), CHECK(`world_unlock` IN ('true', 'false')), CHECK(`remote_download` IN ('true', 'false')));")]
internal class ArcaeaSongs
{
    [NonSerialized] internal static readonly Lazy<ConcurrentDictionary<string, ArcaeaSongs>> SongList;
    [NonSerialized] internal static readonly Lazy<ConcurrentDictionary<string, SongsItem>> SongJsonList;
    [NonSerialized] internal static readonly Lazy<IEnumerable<ArcaeaAlias>> AliasList;

    [NonSerialized] private static readonly ConcurrentDictionary<string, string> Abbreviations = new();

    [NonSerialized] internal static readonly List<(string sid, int dif, int rating)> SortByRating;

    private static readonly ConcurrentDictionary<string, ArcaeaSongs?[]> Aliascache = new();

    static ArcaeaSongs()
    {
        SongList = new(() => new(DatabaseManager.Song.SelectAll<ArcaeaSongs>().ToDictionary(i => i.SongId, i => i)));
        AliasList = new(DatabaseManager.Song.SelectAll<ArcaeaAlias>());
        foreach (var arcaeaSongs in SongList.Value.Values) arcaeaSongs.Init();
        SongJsonList
            = new(() => new(SongList.Value.Select(i => i.Value.ToJson(false)).ToDictionary(i => i.Id, i => i)));
        SortByRating = new();
        Sort();
    }

    [PrimaryKey] [Column("sid")] public string SongId { get; set; } = "";

    [Column("name_en")] public string SongnameEn { get; set; } = "";

    [Column("name_jp")] public string SongnameJp { get; set; } = "";

    [Column("bpm")] public string Bpm { get; set; } = "";

    [Column("bpm_base")] public double BpmBase { get; set; }

    [Column("pakset")] public string Pakset { get; set; } = "";

    [Column("artist")] public string Artist { get; set; } = "";

    [Column("time")] public int Time { get; set; }

    [Column("side")] public int Side { get; set; }

    [Column("date")] public long Date { get; set; }

    [Column("version")] public string Version { get; set; } = "";

    [Column("world_unlock")] public string WorldUnlock { get; set; } = "false";

    [Column("remote_download")] public string RemoteDownload { get; set; } = "false";

    [Column("notes_pst")] public int PstNote { get; set; }

    [Column("notes_prs")] public int PrsNote { get; set; }

    [Column("notes_ftr")] public int FtrNote { get; set; }

    [Column("notes_byn")] public int BynNote { get; set; } = -1;

    [Column("rating_pst")] public int PstRating { get; set; }

    [Column("rating_prs")] public int PrsRating { get; set; }

    [Column("rating_ftr")] public int FtrRating { get; set; }

    [Column("rating_byn")] public int BynRating { get; set; } = -1;

    [Column("chart_designer_pst")] public string ChartDesignerPst { get; set; } = "";
    [Column("chart_designer_prs")] public string ChartDesignerPrs { get; set; } = "";
    [Column("chart_designer_ftr")] public string ChartDesignerFtr { get; set; } = "";
    [Column("chart_designer_byn")] public string ChartDesignerByn { get; set; } = "";

    [Column("jacket_designer_pst")] public string JacketDesignerPst { get; set; } = "";
    [Column("jacket_designer_prs")] public string JacketDesignerPrs { get; set; } = "";
    [Column("jacket_designer_ftr")] public string JacketDesignerFtr { get; set; } = "";
    [Column("jacket_designer_byn")] public string JacketDesignerByn { get; set; } = "";


    [Column("jacket_override_pst")] public string JacketOverridePst { get; set; } = "false";
    [Column("jacket_override_prs")] public string JacketOverridePrs { get; set; } = "false";
    [Column("jacket_override_ftr")] public string JacketOverrideFtr { get; set; } = "false";
    [Column("jacket_override_byn")] public string JacketOverrideByn { get; set; } = "false";

    [Ignore] internal List<int> Ratings { get; set; } = null!;

    private void Init()
    {
        Ratings = new() { PstRating, PrsRating, FtrRating, BynRating };
        Abbreviations.TryAdd(SongId, GetAbbreviation(SongnameEn));
        if (!string.IsNullOrEmpty(SongnameJp)) Abbreviations.TryAdd(SongId, GetAbbreviation(SongnameJp));
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

            foreach (var (sid, item) in SongList.Value)
            {
                SortByRating.Add((sid, 0, item.PstRating));
                SortByRating.Add((sid, 1, item.PrsRating));
                SortByRating.Add((sid, 2, item.FtrRating));
                if (item.BynRating > 0) SortByRating.Add((sid, 3, item.BynRating));
            }

            SortByRating.Sort((tuple, valueTuple) => valueTuple.Item3 - tuple.Item3);
        }
    }

    internal static ArcaeaSongs GetFromJson(SongsItem item) =>
        new()
        {
            SongId = item.Id,
            SongnameEn = item.TitleLocalized.En,
            SongnameJp = item.TitleLocalized.Ja,
            Bpm = item.Bpm,
            BpmBase = item.BpmBase,
            Pakset = item.Set,
            Artist = item.Artist,
            Side = item.Side,
            Time = 0,
            Date = item.Date,
            Version = item.Version,
            WorldUnlock = item.WorldUnlock
                ? "true"
                : "false",
            RemoteDownload = item.NeedDownload
                ? "true"
                : "false",
            ChartDesignerPst = item.Difficulties[0].ChartDesigner,
            ChartDesignerPrs = item.Difficulties[1].ChartDesigner,
            ChartDesignerFtr = item.Difficulties[2].ChartDesigner,
            ChartDesignerByn = item.Difficulties.Count < 4
                ? ""
                : item.Difficulties[3].ChartDesigner,
            JacketDesignerPst = item.Difficulties[0].JacketDesigner,
            JacketDesignerPrs = item.Difficulties[1].JacketDesigner,
            JacketDesignerFtr = item.Difficulties[2].JacketDesigner,
            JacketDesignerByn = item.Difficulties.Count < 4
                ? ""
                : item.Difficulties[3].JacketDesigner,
            JacketOverridePst = item.Difficulties[0].JacketOverride
                ? "true"
                : "false",
            JacketOverridePrs = item.Difficulties[1].JacketOverride
                ? "true"
                : "false",
            JacketOverrideFtr = item.Difficulties[2].JacketOverride
                ? "true"
                : "false",
            JacketOverrideByn = item.Difficulties.Count < 4
                ? "false"
                : item.Difficulties[3].JacketOverride
                    ? "true"
                    : "false"
        };

    public SongsItem ToJson(bool usejsonlist = true)
    {
        if (usejsonlist && SongJsonList.Value.ContainsKey(SongId)) return SongJsonList.Value[SongId];

        var obj = new SongsItem
                  {
                      Id = SongId,
                      TitleLocalized = new()
                                       {
                                           En = SongnameEn,
                                           Ja = (SongnameJp == ""
                                               ? null
                                               : SongnameJp)!
                                       },
                      Bpm = Bpm,
                      BpmBase = BpmBase,
                      Set = Pakset,
                      Package = PackageInfo.GetById(Pakset)?.Name,
                      Artist = Artist,
                      Side = Side,
                      Time = Time,
                      Date = Date,
                      Version = Version,
                      WorldUnlock = WorldUnlock == "true",
                      NeedDownload = RemoteDownload == "true",
                      Difficulties = new()
                                     {
                                         new()
                                         {
                                             RatingClass = 0,
                                             ChartDesigner = ChartDesignerPst,
                                             JacketDesigner = JacketDesignerPst,
                                             JacketOverride = JacketOverridePst == "true",
                                             RealRating = PstRating,
                                             TotalNotes = PstNote
                                         },
                                         new()
                                         {
                                             RatingClass = 1,
                                             ChartDesigner = ChartDesignerPrs,
                                             JacketDesigner = JacketDesignerPrs,
                                             JacketOverride = JacketOverridePrs == "true",
                                             RealRating = PrsRating,
                                             TotalNotes = PrsNote
                                         },
                                         new()
                                         {
                                             RatingClass = 2,
                                             ChartDesigner = ChartDesignerFtr,
                                             JacketDesigner = JacketDesignerFtr,
                                             JacketOverride = JacketOverrideFtr == "true",
                                             RealRating = FtrRating,
                                             TotalNotes = FtrNote
                                         }
                                     }
                  };

        if (BynRating > 0)
            obj.Difficulties.Add(new()
                                 {
                                     RatingClass = 3,
                                     ChartDesigner = ChartDesignerByn,
                                     JacketDesigner = JacketDesignerByn,
                                     JacketOverride = JacketOverrideByn == "true",
                                     RealRating = BynRating,
                                     TotalNotes = BynNote
                                 });
        return obj;
    }

    internal static ArcaeaSongs? GetById(string? songid)
    {
        if (songid is not null && SongList.Value.TryGetValue(songid, out var value)) return value;
        return null;
    }

    internal static IEnumerable<string> GetAlias(ArcaeaSongs song) =>
        AliasList.Value.Where(i => i.SongId == song.SongId).Select(i => i.Alias);

    internal static ArcaeaSongs?[] GetByAlias(string alias)
    {
        var empty = new ArcaeaSongs[] { };

        if (string.IsNullOrWhiteSpace(alias)) return empty;

        if (Aliascache.ContainsKey(alias)) return Aliascache[alias];

        var data = GetById(alias)
                   ?? GetById(AliasList.Value.FirstOrDefault(c => StringCompareHelper.Equals(c.Alias, alias))?.SongId)
                   ?? SongList.Value.Values.FirstOrDefault(c => StringCompareHelper.Equals(c.SongnameEn, alias));

        if (data != null) return new[] { data };

        var abbrdata = Abbreviations.Where(c => StringCompareHelper.Equals(c.Value, alias)).Select(i => GetById(i.Key))
                                    .Distinct().ToArray();

        if (abbrdata.Length > 0) return abbrdata;

        var dic = new PriorityQueue<string, byte>();
        foreach (var arcaeaAlias in AliasList.Value)
        {
            if (StringCompareHelper.Contains(arcaeaAlias.Alias, alias)) dic.Enqueue(arcaeaAlias.SongId, 1);
            if (StringCompareHelper.Contains(alias, arcaeaAlias.Alias)) dic.Enqueue(arcaeaAlias.SongId, 4);
        }

        foreach (var songdata in SongList.Value.Values)
        {
            if (StringCompareHelper.Contains(songdata.SongId, alias)) dic.Enqueue(songdata.SongId, 2);
            if (StringCompareHelper.Contains(alias, songdata.SongId)) dic.Enqueue(songdata.SongId, 5);

            if (StringCompareHelper.Contains(songdata.SongnameEn, alias)) dic.Enqueue(songdata.SongId, 3);
            if (StringCompareHelper.Contains(alias, songdata.SongnameEn)) dic.Enqueue(songdata.SongId, 6);

            if (!string.IsNullOrEmpty(songdata.SongnameJp))
            {
                if (StringCompareHelper.Contains(songdata.SongnameJp, alias)) dic.Enqueue(songdata.SongId, 3);
                if (StringCompareHelper.Contains(alias, songdata.SongnameJp)) dic.Enqueue(songdata.SongId, 6);
            }
        }

        if (dic.Count == 0) return empty;

        dic.TryDequeue(out var firstobj, out var lowestpriority);

        var ls = new List<ArcaeaSongs> { GetById(firstobj)! };

        while (dic.TryDequeue(out var obj, out var priority) && priority == lowestpriority) ls.Add(GetById(obj)!);

        var result = ls.Distinct().ToArray();
        Aliascache.TryAdd(alias, result);
        return result;
    }

    internal static IEnumerable<(ArcaeaSongs song, int dif)> GetByConstRange(int lowerlimit, int upperlimit)
    {
        return SongList.Value.Values.SelectMany(c => c.Ratings, (c, i) => new { c, i })
                       .Where(t => t.i >= lowerlimit && t.i <= upperlimit).Select(t => (t.c, t.c.Ratings.IndexOf(t.i)));
    }

    internal static (ArcaeaSongs?, int) RandomSong(int lowerlimit, int upperlimit)
    {
        var ls = GetByConstRange(lowerlimit, upperlimit).ToArray();
        return ls.Length > 0
            ? RandomHelper.GetRandomItem(ls)
            : (null, -1);
    }

    internal static ArcaeaSongs RandomSong() => RandomHelper.GetRandomItem(SongList.Value.Values.ToArray());

    internal static void Insert(SongsItem songsItem)
    {
        if (GetById(songsItem.Id) is null)
        {
            var item = GetFromJson(songsItem);
            item.Init();
            SongList.Value.TryAdd(songsItem.Id, item);
            SongJsonList.Value.TryAdd(songsItem.Id, item.ToJson());
            DatabaseManager.Song.Value.Insert(item);
        }
    }

    internal static void UpdateRating(FriendsItem friend)
    {
        var songId = friend.RecentScore[0].SongID;
        var difficulty = friend.RecentScore[0].Difficulty;
        var @const = (int)Math.Round(CalcSongConst(friend.RecentScore[0].Score, friend.RecentScore[0].Rating) * 10);
        if (@const == 0) return;

        var item = GetById(songId);

        // not update yet
        if (item is null) return;

        switch (difficulty)
        {
            case 0:
                if (item.PstRating != @const)
                {
                    item.PstRating = @const;
                    item.PstNote = friend.RecentScore[0].MissCount + friend.RecentScore[0].NearCount
                                                                   + friend.RecentScore[0].PerfectCount;
                    SongJsonList.Value[item.SongId] = item.ToJson();
                    DatabaseManager.Song.Update(item);
                    Sort();
                }

                return;

            case 1:
                if (item.PrsRating != @const)
                {
                    item.PrsRating = @const;
                    item.PrsNote = friend.RecentScore[0].MissCount + friend.RecentScore[0].NearCount
                                                                   + friend.RecentScore[0].PerfectCount;
                    SongJsonList.Value[item.SongId] = item.ToJson();
                    DatabaseManager.Song.Update(item);
                    Sort();
                }

                return;

            case 2:
                if (item.FtrRating != @const)
                {
                    item.FtrRating = @const;
                    item.FtrNote = friend.RecentScore[0].MissCount + friend.RecentScore[0].NearCount
                                                                   + friend.RecentScore[0].PerfectCount;
                    SongJsonList.Value[item.SongId] = item.ToJson();
                    DatabaseManager.Song.Update(item);
                    Sort();
                }

                return;

            case 3:
                if (item.BynRating != @const)
                {
                    item.BynRating = @const;
                    item.BynNote = friend.RecentScore[0].MissCount + friend.RecentScore[0].NearCount
                                                                   + friend.RecentScore[0].PerfectCount;
                    var path = $"{GlobalConfig.Config.DataRootPath}/sourse/songs/{item.SongId}_3.jpg";
                    item.JacketOverrideByn = File.Exists(path)
                        ? "true"
                        : "false";
                    SongJsonList.Value[item.SongId] = item.ToJson();
                    DatabaseManager.Song.Update(item);
                    Sort();
                }

                return;
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

    internal static class StringCompareHelper
    {
        private static readonly Regex Reg = new(@"\s|\(|\)|（|）", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static bool Contains(string? raw, string? seed) =>
            seed != null && raw != null
                         && Reg.Replace(raw, "").IndexOf(Reg.Replace(seed, ""), StringComparison.OrdinalIgnoreCase)
                         >= 0;

        internal static bool Equals(string? raw, string? seed) =>
            seed != null && raw != null
                         && string.Equals(Reg.Replace(raw, ""), Reg.Replace(seed, ""),
                                          StringComparison.OrdinalIgnoreCase);
    }

    internal static class RandomHelper
    {
        private static readonly Random Random = new();
        internal static T GetRandomItem<T>(T[] ls) => ls[Random.Next(ls.Length)];
    }
}
