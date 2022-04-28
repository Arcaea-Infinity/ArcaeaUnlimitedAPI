using System.Text.RegularExpressions;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

[ApiController]
public partial class PublicApi : ControllerBase
{
    private static readonly ConcurrentApiRequest<string, (UserInfoResponse? infodata, Response? error)>
        UserInfoConcurrent = new();

    private static readonly ConcurrentApiRequest<(string, string, int), (UserBestResponse? bestdata, Response? error)>
        UserBestConcurrent = new();

    private static readonly ConcurrentApiRequest<string, (UserBest30Response? b30data, Response? error)>
        UserBest30Concurrent = new();

    private bool UserAgentCheck() =>
        Config.Whitelist.Any(pattern => Regex.IsMatch(Request.Headers["User-Agent"].ToString(), pattern));

    private static PlayerInfo? QueryPlayerInfo(string? user, string? usercode, out Response? error)
    {
        error = null;

        if (!string.IsNullOrWhiteSpace(usercode))
        {
            if (!int.TryParse(usercode, out var ucode) || ucode is < 0 or > 999999999)
            {
                error = Error.InvalidUsercode;
                return null;
            }

            // use this user code directly
            return PlayerInfo.GetByCode(usercode).FirstOrDefault()
                   ?? new PlayerInfo { Code = usercode.PadLeft(9, '0') };
        }

        if (string.IsNullOrWhiteSpace(user))
        {
            error = Error.InvalidUserNameorCode;
            return null;
        }

        var players = PlayerInfo.GetByAny(user);

        if (players.Count == 0)
        {
            if (!int.TryParse(user, out var ucode) || ucode is < 0 or > 999999999)
            {
                error = Error.UserNotFound;
                return null;
            }

            return new() { Code = user.PadLeft(9, '0') };
        }

        if (players.Count > 1)
        {
            error = Error.TooManyUsers;
            return null;
        }

        return players[0];
    }

    private static ArcaeaSong? QuerySongInfo(string? songname, string? songid, out Response? error)
    {
        error = null;

        if (!string.IsNullOrWhiteSpace(songid)) return ArcaeaCharts.QueryById(songid);

        if (string.IsNullOrWhiteSpace(songname))
        {
            error = Error.InvalidSongNameorID;
            return null;
        }

        var ls = ArcaeaCharts.Query(songname);

        if (ls is null || ls.Count < 1)
        {
            error = Error.SongNotFound;
            return null;
        }

        if (ls.Count > 1)
        {
            error = Error.TooManySongs(ls);
            return null;
        }

        return ls[0];
    }

    private static FriendsItem? RecordPlayers(AccountInfo account, PlayerInfo player, out Response? error)
    {
        error = null;

        var (success, friends) = account.AddFriend(player.Code).Result;

        if (!success || friends is null || friends.Count < 1)
        {
            error = Error.AddFriendFailed;
            return null;
        }

        if (friends.Count > 1)
        {
            error = Error.ClearFriendFailed;
            return null;
        }

        var friend = friends[0];
        friend.Code = player.Code;

        // update user info and recently played
        player.Update(friend);

        // insert new record into database
        if (friend.RecentScore?.Any() == true)
        {
            // Time > 2021/01/01 'cause 616 changed the calc func of ptt in 2020
            if (friend.RecentScore[0].TimePlayed > 1609430400000) ArcaeaCharts.UpdateRating(friend.RecentScore[0]);

            Records.Insert(friend, friend.RecentScore[0]);
        }

        return friend;
    }
}
