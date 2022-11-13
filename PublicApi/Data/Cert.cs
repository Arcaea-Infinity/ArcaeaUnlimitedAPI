using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [AuthorizationCheck(NotCounted = true)]
    [HttpGet("data/cert")]
    public object GetCert()
        => Success(new { entry = Config.ApiEntry, version = Config.Appversion, cert = ArcaeaFetch.Base64Cert, password = Config.CertPassword });
}
