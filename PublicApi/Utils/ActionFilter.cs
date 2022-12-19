using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal sealed class APIStatusCheck : ActionFilterAttribute
{
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
    }
}

internal sealed class AuthorizationCheck : ActionFilterAttribute
{
    public bool Strict { get; init; }

    private static bool TokenCheck(HttpContext context, out string token)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authString)) return false;
        var str = authString.ToString();
        if (str.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Tokens.Contains(str[7..]);

        return false;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var currentTokenID = "0000";

        if (TokenCheck(context.HttpContext, out var token))
        {
            currentTokenID = token[..4];
            QueryCounter.RecordQuery(currentTokenID);
        }
        else if (Strict)
        {
            context.Result = new ObjectResult(null) { StatusCode = 404 };
            return;
        }
        else if (RateLimiter.IsExceeded(context.HttpContext.Connection.RemoteIpAddress?.ToString()))
        {
            context.Result = new ObjectResult(Response.Error.QuotaExceeded) { StatusCode = 429 };
            return;
        }

        context.ActionArguments["currentTokenID"] = currentTokenID;

        base.OnActionExecuting(context);
    }
}
