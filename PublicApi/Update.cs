using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/update")]
    public async Task<object> GetUpdate()
    {
        if (!UserAgentCheck()) return NotFound(null);
        var obj = await Utils.GetLatestVersion();

        return obj is not null
            ? Success(obj)
            : Error.UpdateServiceUnavailable;
    }
}
