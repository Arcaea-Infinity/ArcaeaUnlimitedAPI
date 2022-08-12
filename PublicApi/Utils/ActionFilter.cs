using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal class UserAgentAuth : ActionFilterAttribute
{
    private static bool UserAgentCheck(string ua) => Config.Whitelist.Any(pattern => Regex.IsMatch(ua, pattern));

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!UserAgentCheck(context.HttpContext.Request.Headers.UserAgent.ToString()))
            context.Result = new NotFoundObjectResult(null);

        base.OnActionExecuting(context);
    }
}

internal class UpdateCheck : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (NeedUpdate)  
            context.Result = new JsonResult(Response.Error.NeedUpdate);

        base.OnActionExecuting(context);
    }
}
