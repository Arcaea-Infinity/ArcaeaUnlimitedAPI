using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed class Response
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("content")]
    public object? Content { get; set; }

    internal static JsonResult Success(object data) => new(new Response() { Status = 0, Content = data });

    private static JsonResult Exception(
        int errorCode,
        string message,
        int httpcode = 400,
        object? content = null)
        => new(new Response() { Status = errorCode, Message = message, Content = content }) { StatusCode = httpcode };

    internal static class Error
    {
        /// <summary>
        ///     errorCode = -1
        /// </summary>
        internal static readonly JsonResult InvalidUserNameorCode = Exception(-1, "invalid username or usercode");

        /// <summary>
        ///     errorCode = -2
        /// </summary>
        internal static readonly JsonResult InvalidUsercode = Exception(-2, "invalid usercode");

        /// <summary>
        ///     errorCode = -3
        /// </summary>
        internal static readonly JsonResult UserNotFound = Exception(-3, "user not found");

        /// <summary>
        ///     errorCode = -4
        /// </summary>
        internal static readonly JsonResult TooManyUsers = Exception(-4, "too many users");

        /// <summary>
        ///     errorCode = -5
        /// </summary>
        internal static readonly JsonResult InvalidSongNameorID = Exception(-5, "invalid songname or songid");

        /// <summary>
        ///     errorCode = -6
        /// </summary>
        internal static readonly JsonResult InvalidSongID = Exception(-6, "invalid songid");

        /// <summary>
        ///     errorCode = -7
        /// </summary>
        internal static readonly JsonResult SongNotFound = Exception(-7, "song not recorded");

        /// <summary>
        ///     errorCode = -8
        /// </summary>
        internal static readonly Func<IEnumerable<ArcaeaSong>, JsonResult> TooManySongs = (ls)
            => Exception(-8, "too many songs", 400, new { songs = ls.Select(i => i.SongID) });

        /// <summary>
        ///     errorCode = -9
        /// </summary>
        internal static readonly JsonResult InvalidDifficulty = Exception(-9, "invalid difficulty");

        /// <summary>
        ///     errorCode = -10
        /// </summary>
        internal static readonly JsonResult InvalidRecentOrOverflowNumber = Exception(-10, "invalid recent/overflow number");

        /// <summary>
        ///     errorCode = -11
        /// </summary>
        internal static readonly JsonResult AllocateAccountFailed = Exception(-11, "allocate an arc account failed", 503);

        /// <summary>
        ///     errorCode = -12
        /// </summary>
        internal static readonly JsonResult ClearFriendFailed = Exception(-12, "clear friend failed", 503);

        /// <summary>
        ///     errorCode = -13
        /// </summary>
        internal static readonly JsonResult AddFriendFailed = Exception(-13, "add friend failed", 503);

        /// <summary>
        ///     errorCode = -14
        /// </summary>
        internal static readonly JsonResult NoThisLevel = Exception(-14, "this song has no this level");

        /// <summary>
        ///     errorCode = -15
        /// </summary>
        internal static readonly JsonResult NotPlayedYet = Exception(-15, "not played yet");

        /// <summary>
        ///     errorCode = -16
        /// </summary>
        internal static readonly JsonResult Shadowbanned = Exception(-16, "user got shadowbanned", 503);

        /// <summary>
        ///     errorCode = -17
        /// </summary>
        internal static readonly JsonResult QueryingB30Failed = Exception(-17, "querying best30 failed", 503);

        /// <summary>
        ///     errorCode = -18
        /// </summary>
        internal static readonly JsonResult UpdateServiceUnavailable = Exception(-18, "update service unavailable", 503);

        /// <summary>
        ///     errorCode = -19
        /// </summary>
        internal static readonly JsonResult InvalidPartner = Exception(-19, "invalid partner");

        /// <summary>
        ///     errorCode = -20
        /// </summary>
        internal static readonly JsonResult FileUnavailable = Exception(-20, "file unavailable", 404);

        /// <summary>
        ///     errorCode = -21
        /// </summary>
        internal static readonly JsonResult InvalidRange = Exception(-21, "invalid range");

        /// <summary>
        ///     errorCode = -22
        /// </summary>
        internal static readonly JsonResult InvalidEnd = Exception(-22, "range of rating end smaller than its start");

        /// <summary>
        ///     errorCode = -23
        /// </summary>
        internal static readonly JsonResult BelowTheThreshold = Exception(-23, "potential is below the threshold of querying best30 (7.0)");

        /// <summary>
        ///     errorCode = -24
        /// </summary>
        internal static readonly JsonResult NeedUpdate = Exception(-24, "need to update arcaea, please contact maintainer", 503);

        /// <summary>
        ///     errorCode = -25
        /// </summary>
        internal static readonly JsonResult InvalidVersion = Exception(-25, "invalid version");

        /// <summary>
        ///     errorCode = -26
        /// </summary>
        internal static readonly JsonResult QuotaExceeded = Exception(-26, "daily query quota exceeded", 429);

        /// <summary>
        ///     errorCode = -27
        /// </summary>
        internal static readonly JsonResult IllegalHash = Exception(-27, "illegal hash, please contact maintainer", 503);
    }
}
