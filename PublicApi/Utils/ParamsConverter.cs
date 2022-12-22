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

internal sealed class SongInfoConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new SongInfoParams(context.GetValue("songname"), context.GetValue("songid"));

        context.ActionArguments["song"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = error;
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal sealed class PlayerInfoConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new PlayerInfoParams(context.GetValue("user"), context.GetValue("usercode"));

        context.ActionArguments["player"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = error;
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal sealed class OverflowConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new OverflowParams(context.GetValue("overflow"));

        context.ActionArguments["overflow"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = error;
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal sealed class RecentConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new RecentParams(context.GetValue("recent"));

        context.ActionArguments["recent"] = obj.Validate(out var error);

        if (error != null)
        {
            context.Result = error;
            return;
        }

        base.OnActionExecuting(context);
    }
}

internal sealed class DifficultyConverterAttribute : ActionFilterAttribute
{
    public bool IgnoreError { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var obj = new DifficultyParams(context.GetValue("difficulty"));

        var difficulty = obj.Validate(out var error);

        if ((difficulty < 0 || !IgnoreError) && error != null)
        {
            context.Result = error;
            return;
        }

        context.ActionArguments["difficulty"] = difficulty;

        base.OnActionExecuting(context);
    }
}

internal sealed class ChartConverterAttribute : ActionFilterAttribute
{
    private static bool ChartMissingCheck(ArcaeaSong song, sbyte difficulty)
        => (difficulty == 3 && song.Count < 4) || (song.SongID == "lasteternity" && difficulty != 3);

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var song = (context.ActionArguments["song"] as ArcaeaSong)!;
        var difficulty = (sbyte)context.ActionArguments["difficulty"]!;
        if (ChartMissingCheck(song, difficulty))
        {
            context.Result = Response.Error.NoThisLevel;
            return;
        }

        context.ActionArguments["chart"] = song[difficulty];

        base.OnActionExecuting(context);
    }
}

internal sealed class FileConverterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var file = context.GetValue("file");

        if (!string.IsNullOrWhiteSpace(file))
        {
            if (file.Contains("/"))
            {
                context.Result = Response.Error.FileUnavailable;
                return;
            }

            var fileinfo = new FileInfo($"{GlobalConfig.Config.DataPath}/source/songs/{file}.jpg");

            if (!fileinfo.Exists)
            {
                context.Result = Response.Error.FileUnavailable;
                return;
            }

            context.Result = new PhysicalFileResult(fileinfo.FullName, "image/jpeg");
        }

        base.OnActionExecuting(context);
    }
}
