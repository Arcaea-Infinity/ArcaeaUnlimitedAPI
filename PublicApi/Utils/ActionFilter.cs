using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal class AuthorizationCheck : ActionFilterAttribute
{
    private static bool UserAgentCheck(HttpContext context)
        => UserAgents.Any(pattern => Regex.IsMatch(context.Request.Headers.UserAgent.ToString(), pattern));

    private static bool TokenCheck(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authString)) return false;
        var str = authString.ToString();
        if (str.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Tokens.Contains(str[7..]);

        return false;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (NeedUpdate)
        {
            context.Result = new JsonResult(Response.Error.NeedUpdate);
            return;
        }

        if (!UserAgentCheck(context.HttpContext) && !TokenCheck(context.HttpContext))
            if (RateLimiter.IsExceeded(context.HttpContext.Connection.RemoteIpAddress?.ToString()))
            {
                context.Result = new ObjectResult(null) { StatusCode = 429 };
                return;
            }

        base.OnActionExecuting(context);
    }
}
