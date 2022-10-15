using System.Net;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.PublicApi;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
{
    Log.ExceptionError(eventArgs.Exception.InnerException!);
    eventArgs.SetObserved();
};

ServicePointManager.DefaultConnectionLimit = 64;
ServicePointManager.ReusePort = true;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore)
       .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
            options.SuppressMapClientErrors = true;
        });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options => options.AddDefaultPolicy(i => i.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
var app = builder.Build();
app.UseCors();

app.UseExceptionHandler(build => build.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (ex != null) Log.ExceptionError(ex);
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonConvert.SerializeObject(Response.Error.InternalErrorOccurred));
}));

app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
app.MapControllers();
app.Run();
