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

    public static void ApiError(string resturl) => WriteLog("apierr", $"{resturl}: query failed");

    public static void ApiError(string resturl, ResponseRoot json)
        => WriteLog("apierr", $"{resturl}: code: {json.ErrorCode?.ToString() ?? json.Code}");

    internal static void ApiError(string resturl, int httpstatus)
    {
        if (httpstatus == 429) return;
        WriteLog("apierr", $"{resturl}: httpstatus: {httpstatus}");
    }

    internal static void FunctionError(string function, string content) => WriteLog("functionerr", $"{function}: {content}");

    internal static void ExceptionError(Exception ex) => WriteLog("exception", ex.ToString());

    public static void HttpError(Exception ex, Uri? uri) => WriteLog("http", $"{uri}\n{ex}");

    internal static void RatingLog(
        string songID,
        string songName,
        int difficulty,
        int originalconst,
        int @const)
        => WriteLog("rating", $"{songID} ({songName}) [{difficulty}]  {originalconst} -> {@const}");
}
