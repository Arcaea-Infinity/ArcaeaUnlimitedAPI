using System.Net;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;

AppDomain.CurrentDomain.UnhandledException
    += (_, eventArgs) => Log.ExceptionError((eventArgs.ExceptionObject as Exception)!);
TaskScheduler.UnobservedTaskException += (_, eventArgs) => Log.ExceptionError(eventArgs.Exception.InnerException!);

ServicePointManager.DefaultConnectionLimit = 64;
ServicePointManager.ReusePort = true;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
       .AddNewtonsoftJson(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
                             options.AddDefaultPolicy(i => i.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
var app = builder.Build();
app.UseCors();
app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
app.MapControllers();
app.Run();
