using System.Net;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;
using BackgroundService = ArcaeaUnlimitedAPI.Core.BackgroundService;

TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
{
    Logger.ExceptionError(eventArgs.Exception.InnerException!);
    eventArgs.SetObserved();
};

ServicePointManager.DefaultConnectionLimit = 64;
ServicePointManager.ReusePort = true;

GlobalConfig.Init();
DatabaseManager.Init();
ArcaeaFetch.Init();
BackgroundService.Init();
ConfigWatcher.Init();

GlobalConfig.CheckUpdate();

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

app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = ExceptionHandler.Invoke });

app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
app.MapControllers();
app.Run();
