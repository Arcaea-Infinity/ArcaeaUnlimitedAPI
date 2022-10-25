using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal sealed class AuthorizationCheck : ActionFilterAttribute
{
    private static bool TokenCheck(HttpContext context, out string token)
    {
        token = string.Empty;
        if (!context.Request.Headers.TryGetValue("Authorization", out var authString)) return false;
        var str = authString.ToString();
        if (str.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = str[7..];
            return Tokens.Contains(token);
        }

        return false;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (NeedUpdate)
        {
            context.Result = new JsonResult(Response.Error.NeedUpdate);
            return;
        }

        if (IllegalHash)
        {
            context.Result = new JsonResult(Response.Error.IllegalHash);
            return;
        }

        string currentTokenID;

        if (TokenCheck(context.HttpContext, out var token))
        {
            currentTokenID = token[..4];
            QueryCounter.RecordQuery(currentTokenID);
        }
        else if (RateLimiter.IsExceeded(context.HttpContext.Connection.RemoteIpAddress?.ToString()))
        {
            context.Result = new ObjectResult(Response.Error.QuotaExceeded) { StatusCode = 429 };
            return;
        }
        else
        {
            currentTokenID = "0000";
        }

        context.ActionArguments["currentTokenID"] = currentTokenID;

        base.OnActionExecuting(context);
    }
}
