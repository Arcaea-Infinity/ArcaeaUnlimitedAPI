using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [AuthorizationCheck]
    [HttpGet("/botarcapi/data/cert")]
    public async Task<object> GetCert()
    {
        var certstr = Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(Path.Combine(Config.DataPath, Config.CertFileName)));
        return Success(new { name = Config.CertFileName, cert = certstr });
    }
}
