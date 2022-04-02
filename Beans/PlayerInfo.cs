using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.PublicApi;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

[Serializable]
[Table("players")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE `players` (`uid`  INTEGER NOT NULL,`code` TEXT NOT NULL,`name` TEXT NOT NULL,`ptt` INTEGER DEFAULT -1,`join_date` INTEGER NOT NULL,PRIMARY KEY (`uid` ASC));")]
internal class PlayerInfo
{
    [PrimaryKey] [Column("uid")] public string UserID { get; set; }

    [Column("code")] public string Code { get; set; }

    [Column("name")] public string Name { get; set; }

    [Column("ptt")] public int Potential { get; set; }

    [Column("join_date")] public long JoinDate { get; set; }

    internal static List<PlayerInfo> GetByAny(string user) =>
        DatabaseManager.Player.Where<PlayerInfo>(i => i.Code == user || i.Name == user).ToList();

    internal static List<PlayerInfo> GetByCode(string code) =>
        DatabaseManager.Player.Where<PlayerInfo>(i => i.Code == code).ToList();

    internal void Update(FriendsItem item)
    {
        UserID = item.UserID.ToString();
        Name = item.Name;
        Potential = item.Rating;
        JoinDate = item.JoinDate;
        DatabaseManager.Player.InsertOrReplace(this);
    }
}
