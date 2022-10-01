using System.Linq.Expressions;
using ArcaeaUnlimitedAPI.Core;
using SQLite;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal static class DatabaseManager
{
    internal static readonly Lazy<SQLiteConnection> Song = GetLazyConnection("arcsong"),
                                                    Account = GetLazyConnection("arcaccount"),
                                                    Player = GetLazyConnection("arcplayer"),
                                                    Record = GetLazyConnection("arcrecord"),
                                                    Bests = GetLazyConnection("arcbests"),
                                                    Best30 = GetLazyConnection("arcbest30");

    private static Lazy<SQLiteConnection> GetLazyConnection(string dbName)
        => new(() => new($"{GlobalConfig.Config.DataPath}/database/{dbName}.db",
                         SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    internal static TableQuery<T> Where<T>(this Lazy<SQLiteConnection> connection, Expression<Func<T, bool>> predExpr) where T : new()
        => connection.Value.Table<T>().Where(predExpr);

    internal static IEnumerable<T> SelectAll<T>(this Lazy<SQLiteConnection> connection) where T : new() => connection.Value.Table<T>();

    internal static void Insert<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() => connection.Value.Insert(obj);

    internal static void InsertOrReplace<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() => connection.Value.InsertOrReplace(obj);

    internal static void Update<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() => connection.Value.Update(obj);

    // ReSharper disable once UnusedParameter.Local
    [AttributeUsage(AttributeTargets.Class)]
    internal class CreateTableSqlAttribute : Attribute
    {
        internal CreateTableSqlAttribute(string sql) { }
    }
}
