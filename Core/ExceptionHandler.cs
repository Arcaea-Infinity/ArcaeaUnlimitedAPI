using ArcaeaUnlimitedAPI.PublicApi;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class ExceptionHandler
{
    internal static async Task Invoke(HttpContext context)
    {
        context.Response.StatusCode = 500;

        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (ex == null) return;

        Logger.ExceptionError(ex);
        Console.WriteLine(ex);

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonConvert.SerializeObject(Response.Error.InternalErrorOccurred));
    }
}
