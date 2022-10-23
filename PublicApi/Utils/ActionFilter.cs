﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

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

        if (!TokenCheck(context.HttpContext))
            if (RateLimiter.IsExceeded(context.HttpContext.Connection.RemoteIpAddress?.ToString()))
            {
                context.Result = new ObjectResult(Response.Error.QuotaExceeded) { StatusCode = 429 };
                return;
            }

        base.OnActionExecuting(context);
    }
}
