using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.PublicApi.Params;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal static class ConverterUtils
{
    internal static string GetValue(this ActionExecutingContext context, string key) => context.HttpContext.Request.Query[key].ToString();
}

internal class SongInfoConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new SongInfoParams(context.GetValue("songname"), context.GetValue("songid"));

        context.ActionArguments["song"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = new JsonResult(error);
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal class PlayerInfoConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new PlayerInfoParams(context.GetValue("user"), context.GetValue("usercode"));

        context.ActionArguments["player"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = new JsonResult(error);
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal class OverflowConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new OverflowParams(context.GetValue("overflow"));

        context.ActionArguments["overflow"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = new JsonResult(error);
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal class RecentConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new RecentParams(context.GetValue("recent"));

        context.ActionArguments["recent"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = new JsonResult(error);
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal class DifficultyConverterAttribute : ActionFilterAttribute
{
    private bool IgnoreError { get; }
    
    internal DifficultyConverterAttribute(bool ignore = true)
    {
        IgnoreError = ignore;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new DifficultyParams(context.GetValue("difficulty"));

        context.ActionArguments["difficulty"] = obj.Validate(out var error);

        if (!IgnoreError && error != null)
        {
            context.Result = new JsonResult(error);
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal class ChartConverterAttribute : ActionFilterAttribute
{
    private static bool ChartMissingCheck(ArcaeaSong song, sbyte difficulty)
        => (difficulty == 3 && song.Count < 4) || (song.SongID == "lasteternity" && difficulty != 3);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var song = (context.ActionArguments["song"] as ArcaeaSong)!;
        var difficulty = (sbyte)context.ActionArguments["difficulty"]!;

        if (ChartMissingCheck(song, difficulty))
        {
            context.Result = new JsonResult(Response.Error.NoThisLevel);
            return;
        }

        context.ActionArguments["chart"] = song[difficulty];

        base.OnActionExecuting(context);
    }
}

internal class FileConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var file = context.GetValue("file");

        if (string.IsNullOrWhiteSpace(file) || file.Contains("/")) return;

        var fileinfo = new FileInfo($"{GlobalConfig.Config.DataPath}/source/songs/{file}.jpg");

        if (!fileinfo.Exists)
        {
            context.Result = new JsonResult(Response.Error.FileUnavailable) { StatusCode = 404 };
            return;
        }

        context.Result = new PhysicalFileResult(fileinfo.FullName, "image/jpeg");
        base.OnActionExecuting(context);
    }
}
