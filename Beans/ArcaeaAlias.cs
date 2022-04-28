using ArcaeaUnlimitedAPI.PublicApi;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

[Serializable]
[Table("alias")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE `alias` (`sid` TEXT NOT NULL,`alias` TEXT NOT NULL PRIMARY KEY, FOREIGN KEY(`sid`) REFERENCES `charts`(`song_id`));")]
public class ArcaeaAlias
{
    [PrimaryKey] [Column("alias")] public string Alias { get; set; }
    [Column("sid")] public string SongID { get; set; }
}
