using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

namespace ArcaeaUnlimitedAPI.Core;

internal static class Log
{
    public static void ApiError(string resturl)
    {
        try
        {
            var time = DateTime.Now;
            File.AppendAllText($"{GlobalConfig.Config.DataRootPath}/log/apierr_{time:yyMMdd}.log",
                               $"\n\n{time}\n{resturl}: query failed");
        }
        catch
        {
            Thread.Sleep(2000);
            ApiError(resturl);
        }
    }

    public static void ApiError(string resturl, ResponseRoot json)
    {
        try
        {
            var time = DateTime.Now;
            File.AppendAllText($"{GlobalConfig.Config.DataRootPath}/log/apierr_{time:yyMMdd}.log",
                               $"\n\n{time}\n{resturl}: code: {json.ErrorCode ?? json.Code}");
        }
        catch
        {
            Thread.Sleep(2000);
            ApiError(resturl, json);
        }
    }

    internal static void ApiError(string resturl, int httpstatus)
    {
        try
        {
            var time = DateTime.Now;

            File.AppendAllText($"{GlobalConfig.Config.DataRootPath}/log/apierr_{time:yyMMdd}.log",
                               $"\n\n{time}\n{resturl}: httpstatus: {httpstatus}");
        }
        catch
        {
            Thread.Sleep(2000);
            ApiError(resturl, httpstatus);
        }
    }

    internal static void FunctionError(string function, string content)
    {
        try
        {
            var time = DateTime.Now;

            File.AppendAllText($"{GlobalConfig.Config.DataRootPath}/log/functionerr_{time:yyMMdd}.log",
                               $"\n\n{time}\n{function}: {content}");
        }
        catch
        {
            Thread.Sleep(2000);
            FunctionError(function, content);
        }
    }

    internal static void ExceptionError(Exception ex)
    {
        try
        {
            var time = DateTime.Now;
            File.AppendAllText($"{GlobalConfig.Config.DataRootPath}/log/exception_{time:yyMMdd}.log",
                               $"\n\n{time}\n{ex}");
        }
        catch
        {
            Thread.Sleep(2000);
            ExceptionError(ex);
        }
    }

    public static void HttpError(string message, Uri? uri)
    {
        try
        {
            var time = DateTime.Now;
            File.AppendAllText($"{GlobalConfig.Config.DataRootPath}/log/http_{time:yyMMdd}.log",
                               $"\n\n{time}\n{uri}\n{message}");
        }
        catch
        {
            Thread.Sleep(2000);
            HttpError(message, uri);
        }
    }
}
