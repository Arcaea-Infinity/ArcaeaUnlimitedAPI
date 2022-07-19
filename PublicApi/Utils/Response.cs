using ArcaeaUnlimitedAPI.Beans;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal class Response
{
    [JsonProperty("status")] public int Status { get; set; }
    [JsonProperty("message")] public string? Message { get; set; }
    [JsonProperty("content")] public object? Content { get; set; }

    internal static Response Success(object data) => new() { Status = 0, Content = data };

    private static Response Exception(int errorCode, string message) => new() { Status = errorCode, Message = message };

    internal static class Error
    {
        /// <summary>
        ///     errorCode = -1
        /// </summary>
        internal static readonly Response InvalidUserNameorCode = Exception(-1, "invalid username or usercode");

        /// <summary>
        ///     errorCode = -2
        /// </summary>
        internal static readonly Response InvalidUsercode = Exception(-2, "invalid usercode");

        /// <summary>
        ///     errorCode = -3
        /// </summary>
        internal static readonly Response UserNotFound = Exception(-3, "user not found");

        /// <summary>
        ///     errorCode = -4
        /// </summary>
        internal static readonly Response TooManyUsers = Exception(-4, "too many users");

        /// <summary>
        ///     errorCode = -5
        /// </summary>
        internal static readonly Response InvalidSongNameorID = Exception(-5, "invalid songname or songid");

        /// <summary>
        ///     errorCode = -6
        /// </summary>
        internal static readonly Response InvalidSongID = Exception(-6, "invalid songid");

        /// <summary>
        ///     errorCode = -7
        /// </summary>
        internal static readonly Response SongNotFound = Exception(-7, "song not recorded");

        /// <summary>
        ///     errorCode = -9
        /// </summary>
        internal static readonly Response InvalidDifficulty = Exception(-9, "invalid difficulty");

        /// <summary>
        ///     errorCode = -10
        /// </summary>
        internal static readonly Response InvalidRecentOrOverflowNumber
            = Exception(-10, "invalid recent/overflow number");

        /// <summary>
        ///     errorCode = -11
        /// </summary>
        internal static readonly Response AllocateAccountFailed = Exception(-11, "allocate an arc account failed");

        /// <summary>
        ///     errorCode = -12
        /// </summary>
        internal static readonly Response ClearFriendFailed = Exception(-12, "clear friend failed");

        /// <summary>
        ///     errorCode = -13
        /// </summary>
        internal static readonly Response AddFriendFailed = Exception(-13, "add friend failed");

        /// <summary>
        ///     errorCode = -14
        /// </summary>
        internal static readonly Response NoThisLevel = Exception(-14, "this song has no this level");

        /// <summary>
        ///     errorCode = -15
        /// </summary>
        internal static readonly Response NotPlayedYet = Exception(-15, "not played yet");

        /// <summary>
        ///     errorCode = -16
        /// </summary>
        internal static readonly Response Shadowbanned = Exception(-16, "user got shadowbanned");

        /// <summary>
        ///     errorCode = -17
        /// </summary>
        internal static readonly Response QueryingB30Failed = Exception(-17, "querying best30 failed");

        /// <summary>
        ///     errorCode = -18
        /// </summary>
        internal static readonly Response UpdateServiceUnavailable = Exception(-18, "update service unavailable");

        /// <summary>
        ///     errorCode = -19
        /// </summary>
        internal static readonly Response InvalidPartner = Exception(-19, "invalid partner");

        /// <summary>
        ///     errorCode = -20
        /// </summary>
        internal static readonly Response FileUnavailable = Exception(-20, "file unavailable");

        /// <summary>
        ///     errorCode = -21
        /// </summary>
        internal static readonly Response InvalidRange = Exception(-21, "invalid range");

        /// <summary>
        ///     errorCode = -22
        /// </summary>
        internal static readonly Response InvalidEnd = Exception(-22, "range of rating end smaller than its start");

        /// <summary>
        ///     errorCode = -23
        /// </summary>
        internal static readonly Response BelowTheThreshold
            = Exception(-23, "potential is below the threshold of querying best30 (7.0)");

        /// <summary>
        ///     errorCode = -24
        /// </summary>
        internal static readonly Response NeedUpdate
            = Exception(-24, "need to update arcaea, please contact maintainer");

        /// <summary>
        ///     errorCode = -233
        /// </summary>
        internal static readonly Response InternalErrorOccurred = Exception(-233, "internal error occurred");

        /// <summary>
        ///     errorCode = -8
        /// </summary>
        internal static Response TooManySongs(IEnumerable<ArcaeaSong> ls) =>
            new() { Status = -8, Message = "too many records", Content = new { songs = ls.Select(i => i.SongID) } };
    }
}
