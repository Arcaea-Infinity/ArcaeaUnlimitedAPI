using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal class Auth : ActionFilterAttribute
{
    private static bool UserAgentCheck(HttpContext context) =>
        UserAgents.Any(pattern => Regex.IsMatch(context.Request.Headers.UserAgent.ToString(), pattern));

    private static bool TokenCheck(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authString)) return false;
        var str = authString.ToString();
        if (str.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Tokens.Contains(str[7..]);

        return false;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!UserAgentCheck(context.HttpContext) && !TokenCheck(context.HttpContext))
            if (RateLimiter.IsExceeded(context.HttpContext.Connection.RemoteIpAddress?.ToString()))
                context.Result = new ObjectResult(null) { StatusCode = 429 };

        base.OnActionExecuting(context);
    }
}

internal class UpdateCheck : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (NeedUpdate) context.Result = new ObjectResult(Response.Error.NeedUpdate);

        base.OnActionExecuting(context);
    }
}
