using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

public class PlayData
{
    private const string RangeQuerystr
        = "SELECT score/10000 as fscore, count(*) as count FROM records WHERE song_id = ? AND difficulty = ? AND fscore >= 900 AND potential BETWEEN ? AND ? GROUP BY fscore order by fscore desc;";

    [JsonProperty("fscore")] [Column("fscore")]
    public int FormattedScore { get; set; }

    [JsonProperty("count")] [Column("count")]
    public int? Count { get; set; }

    internal static List<PlayData> Query(
        int potentialstart,
        int potentialend,
        string songid,
        int difficulty)
        => DatabaseManager.Bests.Value.Query<PlayData>(RangeQuerystr, songid, difficulty, potentialstart, potentialend);
}

public class PlayDataArray
{
    private const string RangeQuerystr
        = "SELECT score/10000 as fscore, potential/10 as fpotential, count(*) as count FROM records WHERE song_id = ? AND difficulty = ? AND fscore >= 950 AND fpotential >= 110 GROUP BY fscore, fpotential order by fscore desc;";

    [JsonProperty("fscore")] [Column("fscore")]
    public int FormattedScore { get; set; }

    [JsonProperty("fpotential")] [Column("fpotential")]
    public int FormattedPotential { get; set; }

    [JsonProperty("count")] [Column("count")]
    public int? Count { get; set; }

    internal static List<PlayDataArray> Query(ArcaeaCharts chart)
        => DatabaseManager.Bests.Value.Query<PlayDataArray>(RangeQuerystr, chart.SongID, chart.RatingClass);
}
