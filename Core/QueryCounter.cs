using System.Collections.Concurrent;

namespace ArcaeaUnlimitedAPI.Core;

internal static class QueryCounter
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, long>> FetchCounter = new(), AuaQueryCounter = new();

    private static string DateString => DateTime.Now.ToShortDateString();

    internal static void RecordQuery(string tokenid)
    {
        var date = DateString;

        if (AuaQueryCounter.ContainsKey(date))
        {
            if (AuaQueryCounter[date].ContainsKey(tokenid))
                ++AuaQueryCounter[date][tokenid];
            else
                AuaQueryCounter[date][tokenid] = 1;
        }
        else
        {
            Logger.QueryCount(AuaQueryCounter);
            AuaQueryCounter.Clear();
            AuaQueryCounter[date] = new() { [tokenid] = 1 };
        }
    }

    internal static void RecordFetch(string tokenid)
    {
        var date = DateString;

        if (FetchCounter.ContainsKey(date))
        {
            if (FetchCounter[date].ContainsKey(tokenid))
                ++FetchCounter[date][tokenid];
            else
                FetchCounter[date][tokenid] = 1;
        }
        else
        {
            Logger.FetchCount(FetchCounter);
            FetchCounter.Clear();
            FetchCounter[date] = new() { [tokenid] = 1 };
        }
    }
}
