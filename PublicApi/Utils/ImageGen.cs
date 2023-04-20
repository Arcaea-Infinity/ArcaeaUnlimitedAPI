using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal static class ImageGen
{
    internal static void Init() => _imageClient = new() { BaseAddress = new(Config.ImageGenUrl) };

    private static HttpClient _imageClient = null!;

    internal static Task<HttpResponseMessage> ImageRequestPostAsync(string? requestUri, JsonResult jsonResult)
        => _imageClient.PostAsync(requestUri, new StringContent(JsonConvert.SerializeObject(jsonResult.Value), null, "application/json"));
}
