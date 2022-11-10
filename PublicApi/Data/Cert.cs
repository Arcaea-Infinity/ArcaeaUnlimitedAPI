using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [AuthorizationCheck(NotCounted = true)]
    [HttpGet("data/cert")]
    public object GetCert()
    {
        var certstr = Convert.ToBase64String(System.IO.File.ReadAllBytes(Path.Combine(Config.DataPath, Config.CertFileName)));
        return Success(new { entry = Config.ApiEntry, version = Config.Appversion, cert = certstr, password = Config.CertPassword });
    }
}
