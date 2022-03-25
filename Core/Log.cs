using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

namespace ArcaeaUnlimitedAPI.Core;

internal static class Log
{
    public static async void ApiError(string resturl)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataRootPath}/log/apierr_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{resturl}: query failed");
        }
        catch
        {
            await Task.Delay(2000);
            ApiError(resturl);
        }
    }

    public static async void ApiError(string resturl, ResponseRoot json)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataRootPath}/log/apierr_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{resturl}: code: {json.ErrorCode ?? json.Code}");
        }
        catch
        {
            await Task.Delay(2000);
            ApiError(resturl, json);
        }
    }

    internal static async void ApiError(string resturl, int httpstatus)
    {
        try
        {
            var time = DateTime.Now;

            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataRootPath}/log/apierr_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{resturl}: httpstatus: {httpstatus}");
        }
        catch
        {
            await Task.Delay(2000);
            ApiError(resturl, httpstatus);
        }
    }

    internal static async void FunctionError(string function, string content)
    {
        try
        {
            var time = DateTime.Now;

            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataRootPath}/log/functionerr_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{function}: {content}");
        }
        catch
        {
            await Task.Delay(2000);
            FunctionError(function, content);
        }
    }

    internal static async void ExceptionError(Exception ex)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataRootPath}/log/exception_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{ex}");
        }
        catch
        {
            await Task.Delay(2000);
            ExceptionError(ex);
        }
    }

    public static async void HttpError(string message, Uri? uri)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataRootPath}/log/http_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{uri}\n{message}");
        }
        catch
        {
            await Task.Delay(2000);
            HttpError(message, uri);
        }
    }
}
