using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

namespace ArcaeaUnlimitedAPI.Core;

internal static class Logger
{
    private static readonly object SyncObj = new();

    private static readonly string LogDir = Path.Combine(GlobalConfig.Config.DataPath, "log");

    private static string LogPath(string type, DateTime time) => Path.Combine(LogDir, $"{type}_{time:yyMMdd}.log");

    private static void WriteLog(string type, string msg)
    {
        lock (SyncObj)
        {
            var time = DateTime.Now;
            File.AppendAllText(LogPath(type, time), $"\n\n{time}\n{msg}");
        }
    }

    internal static void ApiError(string resturl) => WriteLog("apierr", $"{resturl}: query failed");

    internal static void ApiError(string resturl, ResponseRoot json)
        => WriteLog("apierr", $"{resturl}: code: {json.ErrorCode?.ToString() ?? json.Code}");

    internal static void ApiError(string resturl, int httpstatus)
    {
        if (httpstatus == 429) return;
        WriteLog("apierr", $"{resturl}: httpstatus: {httpstatus}");
    }

    internal static void FunctionError(string function, string content) => WriteLog("functionerr", $"{function}: {content}");

    internal static void ExceptionError(Exception ex) => WriteLog("exception", ex.ToString());

    internal static void HttpError(Exception ex, Uri? uri) => WriteLog("http", $"{uri}\n{ex}");

    internal static void FetchCount(ConcurrentDictionary<string, ConcurrentDictionary<string, long>> counter)
        => WriteLog("fetchcount", Counter(counter));

    internal static void QueryCount(ConcurrentDictionary<string, ConcurrentDictionary<string, long>> counter)
        => WriteLog("querycount", Counter(counter));

    private static string Counter(ConcurrentDictionary<string, ConcurrentDictionary<string, long>> counter)
    {
        var shortDateString = DateTime.Today.AddDays(-1).ToShortDateString();
        return counter.TryGetValue(shortDateString, out ConcurrentDictionary<string, long>? yestdcounter)
                   ? $"{shortDateString}  {yestdcounter.Values.Sum()}\n{string.Join('\n', yestdcounter.Select(i => $"{i.Key} : {i.Value}"))}"
                   : $"{shortDateString}  0";
    }

    internal static void RatingLog(
        string songID,
        string songName,
        int difficulty,
        int originalconst,
        int @const)
        => WriteLog("rating", $"{songID} ({songName}) [{difficulty}]  {originalconst} -> {@const}");
}
