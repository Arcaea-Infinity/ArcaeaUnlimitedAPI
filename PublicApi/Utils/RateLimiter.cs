using System.Collections.Concurrent;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal static class RateLimiter
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> Counter = new();

    internal static bool IsExceeded(string? ip)
    {
        if(string.IsNullOrWhiteSpace(ip)) return true;
        
        var date = DateTime.UtcNow.ToShortDateString();
        
        if (Counter.ContainsKey(ip) && Counter[ip].ContainsKey(date))
        {
            if (Counter[ip][date] >= Config.Quota) return true;

            ++Counter[ip][date];
            return false;
        }

        Counter[ip] = new() { [date] = 1 };
        return false;
    }
}
