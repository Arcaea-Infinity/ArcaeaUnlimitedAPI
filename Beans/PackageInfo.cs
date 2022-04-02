using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.PublicApi;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

[Serializable]
[Table("packages")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE `packages`(`id` TEXT PRIMARY KEY NOT NULL, `name` TEXT NOT NULL DEFAULT '');")]
internal class PackageInfo
{
    private static Lazy<ConcurrentDictionary<string, PackageInfo>> _list
        = new(() => new(DatabaseManager.Song.SelectAll<PackageInfo>().ToDictionary(i => i.PackageID)));

    [PrimaryKey] [Column("id")] public string PackageID { get; set; }

    [Column("name")] public string Name { get; set; }

    internal static PackageInfo? GetById(string id) =>
        _list.Value.TryGetValue(id, out var value)
            ? value
            : null;
}
