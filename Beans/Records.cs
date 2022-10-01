using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using SQLite;

#pragma warning disable CS8618

namespace ArcaeaUnlimitedAPI.Beans;

[Serializable]
[Table("records")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE `records` (`uid` INTEGER NOT NULL,`potential` INTEGER NOT NULL,`score` INTEGER NOT NULL,`health` INTEGER NOT NULL,`rating` INTEGER NOT NULL,`song_id` TEXT NOT NULL,`modifier` INTEGER NOT NULL,`difficulty` INTEGER NOT NULL,`clear_type` INTEGER NOT NULL,`best_clear_type` INTEGER NOT NULL,`time_played` INTEGER NOT NULL,`near_count` INTEGER NOT NULL,`miss_count` INTEGER NOT NULL,`perfect_count` INTEGER NOT NULL,`shiny_perfect_count` INTEGER NOT NULL, PRIMARY KEY (`uid` ASC, `song_id` ASC, `time_played` ASC));")]
public class Records
{
    [JsonProperty("user_id")] [PrimaryKey] [Column("uid")]
    public int? UserID { get; set; }

    [JsonIgnore] [Column("potential")]
    public int? Potential { get; set; }

    [JsonProperty("score")] [Column("score")]
    public int Score { get; set; }

    [JsonProperty("health")] [Column("health")]
    public int? Health { get; set; }

    [JsonProperty("rating")] [Column("rating")]
    public double Rating { get; set; }

    [JsonProperty("song_id")] [PrimaryKey] [Column("song_id")]
    public string SongID { get; set; }

    [JsonProperty("modifier")] [Column("modifier")]
    public int? Modifier { get; set; }

    [JsonProperty("difficulty")] [Column("difficulty")]
    public int Difficulty { get; set; }

    [JsonProperty("clear_type")] [Column("clear_type")]
    public int? ClearType { get; set; }

    [JsonProperty("best_clear_type")] [Column("best_clear_type")]
    public int? BestClearType { get; set; }

    [JsonProperty("time_played")] [Column("time_played")]
    public long TimePlayed { get; set; }

    [JsonProperty("near_count")] [Column("near_count")]
    public int NearCount { get; set; }

    [JsonProperty("miss_count")] [Column("miss_count")]
    public int MissCount { get; set; }

    [JsonProperty("perfect_count")] [Column("perfect_count")]
    public int PerfectCount { get; set; }

    [JsonProperty("shiny_perfect_count")] [Column("shiny_perfect_count")]
    public int ShinyPerfectCount { get; set; }

    internal static List<Records> Query(int userID, int count)
        => DatabaseManager.Record.Where<Records>(i => i.UserID == userID).OrderByDescending(i => i.TimePlayed).Take(count).ToList();

    public static void Insert(FriendsItem friend, Records record)
    {
        record.UserID = friend.UserID;
        record.Potential = friend.Rating;
        DatabaseManager.Record.InsertOrReplace(record);
    }
}
