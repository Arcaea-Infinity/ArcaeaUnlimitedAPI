using System.Linq.Expressions;
using System.Reflection;
using ArcaeaUnlimitedAPI.Beans;
using SQLite;

namespace ArcaeaUnlimitedAPI.Core;

internal static class DatabaseManager
{
    internal static readonly Lazy<SQLiteConnection> Song = GetLazyConnection("arcsong"),
                                                    Account = GetLazyConnection("arcaccount"),
                                                    Player = GetLazyConnection("arcplayer"),
                                                    Record = GetLazyConnection("arcrecord"),
                                                    // Bests = GetLazyConnection("arcbests"),
                                                    Best30 = GetLazyConnection("arcbest30");

    internal static void Init()
    {
        Song.Value.Execute(GetSql<ArcaeaCharts>());
        Song.Value.Execute(GetSql<PackageInfo>());
        Song.Value.Execute(GetSql<ArcaeaAlias>());
        Account.Value.Execute(GetSql<AccountInfo>());
        Player.Value.Execute(GetSql<PlayerInfo>());
        Record.Value.Execute(GetSql<Records>());
        // Bests.Value.Execute(GetSql<Records>());
        Best30.Value.Execute(GetSql<UserBest30Response>());
    }

    private static string GetSql<T>() => (typeof(T).GetCustomAttribute(typeof(CreateTableSqlAttribute)) as CreateTableSqlAttribute)?.Sql!;

    private static Lazy<SQLiteConnection> GetLazyConnection(string dbName)
        => new(() => new($"{GlobalConfig.Config.DataPath}/database/{dbName}.db",
                         SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache | SQLiteOpenFlags.FullMutex));

    internal static TableQuery<T> Where<T>(this Lazy<SQLiteConnection> connection, Expression<Func<T, bool>> predExpr) where T : new()
        => connection.Value.Table<T>().Where(predExpr);

    internal static IEnumerable<T> SelectAll<T>(this Lazy<SQLiteConnection> connection) where T : new() => connection.Value.Table<T>();

    internal static void Insert<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() => connection.Value.Insert(obj);

    internal static void InsertOrReplace<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() => connection.Value.InsertOrReplace(obj);

    internal static void Update<T>(this Lazy<SQLiteConnection> connection, T obj) where T : new() => connection.Value.Update(obj);

    [AttributeUsage(AttributeTargets.Class)]
    internal class CreateTableSqlAttribute : Attribute
    {
        internal readonly string Sql;

        internal CreateTableSqlAttribute(string sql)
        {
            Sql = sql;
        }
    }
}
