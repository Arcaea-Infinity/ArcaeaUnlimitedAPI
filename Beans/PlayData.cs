using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

public class PlayData
{
    private const string RangeQuerystr
        = "SELECT score/10000 as fscore, count(*) as count FROM records WHERE potential BETWEEN ? AND ? AND song_id = ? AND difficulty = ? AND fscore >= 900 GROUP BY fscore order by fscore desc;";

    [JsonProperty("fscore")] [Column("fscore")]
    public int FormattedScore { get; set; }

    [JsonProperty("count")] [Column("count")]
    public int? Count { get; set; }

    internal static IEnumerable<object> Query(int potentialstart, int potentialend, string songid, int difficulty) =>
        DatabaseManager.Bests.Value.Query<PlayData>(RangeQuerystr, potentialstart, potentialend, songid, difficulty);
}
