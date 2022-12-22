using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [EnableCors]
    [HttpGet("update")]
    [HttpGet("data/update")]
    public async Task<JsonResult> GetUpdate()
    {
        var obj = await Utils.GetLatestVersion();
        return obj is not null ? Success(obj) : Error.UpdateServiceUnavailable;
    }
}
