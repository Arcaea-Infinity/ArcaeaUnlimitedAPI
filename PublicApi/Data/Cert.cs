using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [APIStatusCheck(Order = 0)]
    [HttpGet("data/cert")]
    public JsonResult GetCert()
        => Success(new { entry = Config.ApiEntry, version = Config.Appversion, cert = ArcaeaFetch.Base64Cert, password = Config.CertPassword });
}
