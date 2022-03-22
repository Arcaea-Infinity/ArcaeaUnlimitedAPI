using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/update")]
    public object GetUpdate()
    {
        if (!UserAgentCheck()) return NotFound(null);
        var obj = Core.Utils.GetLatestVersion().Result;

        return obj is not null
            ? Success(obj)
            : Error.UpdateServiceUnavailable;
    }
}
