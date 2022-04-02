using System.Linq.Expressions;
using ArcaeaUnlimitedAPI.Core;
using SQLite;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal static class DatabaseManager
{
    internal static readonly Lazy<SQLiteConnection> Song = new(() => new($"{DatabaseDir}/arcsong.db",
                                                                         SQLiteOpenFlags.ReadWrite
                                                                         | SQLiteOpenFlags.SharedCache
                                                                         | SQLiteOpenFlags.FullMutex));

    internal static readonly Lazy<SQLiteConnection> Account
        = new(() => new($"{DatabaseDir}/arcaccount.db",
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    internal static readonly Lazy<SQLiteConnection> Player
        = new(() => new($"{DatabaseDir}/arcplayer.db",
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    internal static readonly Lazy<SQLiteConnection> Record
        = new(() => new($"{DatabaseDir}/arcrecord.db",
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    internal static readonly Lazy<SQLiteConnection> Bests
        = new(() => new($"{DatabaseDir}/arcbests.db",
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    internal static readonly Lazy<SQLiteConnection> Best30
        = new(() => new($"{DatabaseDir}/arcbest30.db",
                        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    private static readonly string DatabaseDir = $"{GlobalConfig.Config.DataPath}/database";

    internal static TableQuery<T> Where<T>(this Lazy<SQLiteConnection> connection, Expression<Func<T, bool>> predExpr)
        where T : new() =>
        connection.Value.Table<T>().Where(predExpr);

    internal static IEnumerable<T> SelectAll<T>(this Lazy<SQLiteConnection> connection) where T : new() =>
        connection.Value.Table<T>();

    internal static void Insert<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() =>
        connection.Value.Insert(obj);

    internal static void InsertOrReplace<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() =>
        connection.Value.InsertOrReplace(obj);

    internal static void Update<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() =>
        connection.Value.Update(obj);

    // ReSharper disable once UnusedParameter.Local
    [AttributeUsage(AttributeTargets.Class)]
    internal class CreateTableSqlAttribute : Attribute
    {
        internal CreateTableSqlAttribute(string sql) { }
    }
}
