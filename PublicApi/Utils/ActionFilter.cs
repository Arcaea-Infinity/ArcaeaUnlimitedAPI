﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal sealed class APIStatusCheck : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (NeedUpdate)
        {
            context.Result = Response.Error.NeedUpdate;
            return;
        }

        if (IllegalHash)
        {
            context.Result = Response.Error.IllegalHash;
            return;
        }
    }
}

internal sealed class AuthorizationCheck : ActionFilterAttribute
{
    private static bool TokenCheck(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authString)) return false;
        var str = authString.ToString();
        if (str.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Tokens.Contains(str[7..]);

        return false;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var currentTokenID = "0000";

        if (!TokenCheck(context.HttpContext) && RateLimiter.IsExceeded(context.HttpContext.Connection.RemoteIpAddress?.ToString()))
        {
            context.Result = Response.Error.QuotaExceeded;
            return;
        }

        context.ActionArguments["currentTokenID"] = currentTokenID;

        base.OnActionExecuting(context);
    }
}
