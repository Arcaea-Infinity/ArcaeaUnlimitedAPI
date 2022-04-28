using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/update")]
    public async Task<object> GetUpdate()
    {
        var obj = await Utils.GetLatestVersion();

        return obj is not null
            ? Success(obj)
            : Error.UpdateServiceUnavailable;
    }
}
