using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

namespace ArcaeaUnlimitedAPI.Core;

internal static class Log
{
    public static async void ApiError(string resturl)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/apierr_{time:yyMMdd}.log", $"\n\n{time}\n{resturl}: query failed");
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
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/apierr_{time:yyMMdd}.log",
                                          $"\n\n{time}\n{resturl}: code: {json.ErrorCode?.ToString() ?? json.Code}");
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

            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/apierr_{time:yyMMdd}.log",
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

            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/functionerr_{time:yyMMdd}.log", $"\n\n{time}\n{function}: {content}");
        }
        catch
        {
            await Task.Delay(2000);
            FunctionError(function, content);
        }
    }

    internal static async void FunctionLog(string function, string content)
    {
        try
        {
            var time = DateTime.Now;

            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/functionlog_{time:yyMMdd}.log", $"\n\n{time}\n{function}: {content}");
        }
        catch
        {
            await Task.Delay(2000);
            FunctionLog(function, content);
        }
    }

    internal static async void RatingLog(
        string songID,
        string songName,
        int difficulty,
        int originalconst,
        int @const)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/rating.log",
                                          $"\n\n{time}\n{songID} ({songName}) [{difficulty}]  {originalconst} -> {@const}");
        }
        catch
        {
            await Task.Delay(2000);
            RatingLog(songID, songName, difficulty, originalconst, @const);
        }
    }

    internal static async void ExceptionError(Exception ex)
    {
        try
        {
            var time = DateTime.Now;
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/exception_{time:yyMMdd}.log", $"\n\n{time}\n{ex}");
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
            await File.AppendAllTextAsync($"{GlobalConfig.Config.DataPath}/log/http_{time:yyMMdd}.log", $"\n\n{time}\n{uri}\n{message}");
        }
        catch
        {
            await Task.Delay(2000);
            HttpError(message, uri);
        }
    }
}
