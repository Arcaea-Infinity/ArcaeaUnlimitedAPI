using System.Security.Cryptography;
using System.Text;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/connect")]
    public object GetConnect()
    {
        if (!UserAgentCheck()) return NotFound(null);

        var now = DateTime.UtcNow;
        const string p = "qwertyuiopasdfghjklzxcvbnm1234567890";
        var v = BitConverter
                .ToString(MD5.Create()
                             .ComputeHash(Encoding.ASCII
                                                  .GetBytes($"{now.Year}ori{now.Month - 1}wol{now.Day}oihs{now.Day}otas")))
                .ToLower().Replace("-", "");
        var r = v.Aggregate("", (current, t) => current + p[t % 36]);

        System.IO.File.WriteAllText("config.json", JsonConvert.SerializeObject(GlobalConfig.Config));
        return Success($"{r[1]}{r[20]}{r[4]}{r[30]}{r[2]}{r[11]}{r[23]}");
    }
}
