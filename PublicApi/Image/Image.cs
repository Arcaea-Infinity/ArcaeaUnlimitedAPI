using System.Net;
using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.ImageGen;
using static ArcaeaUnlimitedAPI.PublicApi.Response.Error;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    private static async Task<IActionResult> FileStreamResult(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.OK) return ImageServerError;

        var stream = new MemoryStream();
        await using var readAsStreamAsync = await response.Content.ReadAsStreamAsync();
        await readAsStreamAsync.CopyToAsync(stream);
        stream.Position = 0;
        return new FileStreamResult(stream, "image/jpeg");
    }

    [APIStatusCheck(Order = 0)]
    [AuthorizationCheck(Order = 1)]
    [PlayerInfoConverter(Order = 2)]
    [SongInfoConverter(Order = 3)]
    [DifficultyConverter(Order = 4, IgnoreError = false)]
    [ChartConverter(Order = 5)]
    [HttpGet("image/user/best")]
    public async Task<IActionResult> GetUserBestImage(
        [BindNever] [FromQuery] PlayerInfo player,
        [BindNever] [FromQuery] ArcaeaCharts chart,
        [BindNever] string currentTokenID,
        int imgVersion = 1,
        bool withrecent = false,
        bool withsonginfo = false)
    {
        var jsonResult = await GetUserBest(player, chart, currentTokenID, withrecent, withsonginfo);
        if (jsonResult.StatusCode != 200) return jsonResult;

        using var response = await ImageRequestPostAsync($"user/best?imgVersion={imgVersion}", jsonResult);
        return await FileStreamResult(response);
    }

    [APIStatusCheck(Order = 0)]
    [AuthorizationCheck(Order = 1)]
    [PlayerInfoConverter(Order = 2)]
    [RecentConverter(Order = 3)]
    [HttpGet("image/user/info")]
    public async Task<IActionResult> GetUserInfoImage(
        [BindNever] PlayerInfo player,
        [BindNever] int recent,
        [BindNever] string currentTokenID,
        int imgVersion = 1,
        bool withsonginfo = false)
    {
        var jsonResult = await GetUserInfo(player, recent, currentTokenID, withsonginfo);
        if (jsonResult.StatusCode != 200) return jsonResult;

        using var response = await ImageRequestPostAsync($"user/info?imgVersion={imgVersion}", jsonResult);
        return await FileStreamResult(response);
    }

    [APIStatusCheck(Order = 0)]
    [AuthorizationCheck(Order = 1)]
    [PlayerInfoConverter(Order = 2)]
    [OverflowConverter(Order = 3)]
    [HttpGet("image/user/best30")]
    public async Task<IActionResult> GetUserBest30Image(
        [BindNever] PlayerInfo player,
        [BindNever] int overflow,
        [BindNever] string currentTokenID,
        int imgVersion = 1,
        bool withrecent = false,
        bool withsonginfo = false)
    {
        var jsonResult = await GetUserBest30(player, overflow, currentTokenID, withrecent, withsonginfo);
        if (jsonResult.StatusCode != 200) return jsonResult;

        using var response = await ImageRequestPostAsync($"user/best30?imgVersion={imgVersion}", jsonResult);
        return await FileStreamResult(response);
    }
}
