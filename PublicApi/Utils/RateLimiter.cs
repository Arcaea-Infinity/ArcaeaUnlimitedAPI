using System.Collections.Concurrent;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal static class RateLimiter
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<DateTime, byte>> Counter = new();

    internal static bool IsExceeded(string ip)
    {
        var date = DateTime.UtcNow.Date;
        if (!Counter.ContainsKey(ip))
        {
            Counter[ip] = new() { [date] = 1 };
            return false;
        }

        if (!Counter[ip].ContainsKey(date))
        {
            Counter[ip].Clear();
            Counter[ip][date] = 1;
            return false;
        }

        if (Counter[ip][date] > Config.Quota) return true;

        lock (Counter[ip])
        {
            Counter[ip][date]++;
            return false;
        }
    }
}
