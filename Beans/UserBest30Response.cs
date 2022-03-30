using System.Text;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.Json.Songlist;
using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

[Serializable]
[Table("cache")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE `cache` (`uid` INTEGER NOT NULL,`last_played`  INTEGER NOT NULL DEFAULT 0,`best30_avg`   INTEGER NOT NULL DEFAULT 0,`recent10_avg` INTEGER NOT NULL DEFAULT 0,`best30_list`  TEXT DEFAULT '',`best30_overflow`  TEXT DEFAULT '',PRIMARY KEY (`uid` ASC));")]
public class UserBest30Response
{
    [JsonIgnore] [PrimaryKey] [Column("uid")]
    public int UserID { get; set; }

    [JsonIgnore] [Column("last_played")] public long LastPlayed { get; set; }

    [JsonProperty("best30_avg")] [Column("best30_avg")]
    public double Best30Avg { get; set; }

    [JsonProperty("recent10_avg")] [Column("recent10_avg")]
    public double Recent10Avg { get; set; }

    [JsonProperty("account_info")] [Ignore]
    public FriendsItem AccountInfo { get; set; }

    [JsonIgnore] [Column("best30_list")] public string Best30ListStr { get; set; }

    [JsonIgnore] [Column("best30_overflow")]
    public string Best30OverflowStr { get; set; }

    [JsonProperty("best30_list")] [Ignore] public List<Records>? Best30List { get; set; }

    [JsonProperty("best30_overflow")] [Ignore]
    public List<Records>? Best30Overflow { get; set; }

    [JsonProperty("best30_songinfo")] [Ignore]
    public IEnumerable<SongItem>? Best30Songinfo { get; set; }

    [JsonProperty("best30_overflow_songinfo")] [Ignore]
    public IEnumerable<SongItem>? Best30OverflowSonginfo { get; set; }

    [JsonProperty("recent_score")] [Ignore]
    public Records? RecentScore { get; set; }

    [JsonProperty("recent_songinfo")] [Ignore]
    public SongItem? RecentSonginfo { get; set; }

    internal static UserBest30Response? GetById(int userid)
    {
        var cache = DatabaseManager.Best30.Where<UserBest30Response>(i => i.UserID == userid).FirstOrDefault();
        if (cache is null) return null;
        cache.Best30List = SerializeHelper.Deserialize<List<Records>>(cache.Best30ListStr);
        cache.Best30Overflow = SerializeHelper.Deserialize<List<Records>>(cache.Best30OverflowStr);
        return cache;
    }

    internal static void Update(UserBest30Response obj)
    {
        obj.Best30ListStr = SerializeHelper.Serialize(obj.Best30List!);
        obj.Best30OverflowStr = SerializeHelper.Serialize(obj.Best30Overflow!);
        DatabaseManager.Best30.InsertOrReplace(obj);
    }

    internal static class SerializeHelper
    {
        internal static string Serialize<T>(T obj) where T : class, new() =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));

        internal static T Deserialize<T>(string str) where T : class, new() =>
            JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(Convert.FromBase64String(str))) ?? new T();
    }
}
