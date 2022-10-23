using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.Core;
using SQLite;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

[Serializable]
[Table("packages")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE IF NOT EXISTS `packages`(`id` TEXT PRIMARY KEY NOT NULL, `name` TEXT NOT NULL DEFAULT '', FOREIGN KEY(`id`) REFERENCES `charts`(`set`));")]
internal sealed class PackageInfo
{
    private static Lazy<ConcurrentDictionary<string, string>> _list
        = new(() => new(DatabaseManager.Song.SelectAll<PackageInfo>().ToDictionary(i => i.PackageID, i => i.Name)));

    [PrimaryKey] [Column("id")]
    public string PackageID { get; set; }

    [Column("name")]
    public string Name { get; set; }

    internal static string? GetNameById(string id) => _list.Value.TryGetValue(id, out var value) ? value : null;

    internal static void Insert(PackageInfo info)
    {
        _list.Value.TryAdd(info.PackageID, info.Name);
        DatabaseManager.Song.Insert(info);
    }
}
