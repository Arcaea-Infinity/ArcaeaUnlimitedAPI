using Microsoft.AspNetCore.Mvc;

namespace ArcaeaUnlimitedAPI.PublicApi.Params;

internal interface IParams<out T>
{
    public T? Validate(out JsonResult? error);
}
