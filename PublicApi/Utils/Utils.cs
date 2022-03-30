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

    private static readonly ConcurrentApiRequest<string, (UserBestResponse? bestdata, Response? error)>
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

        if (players.Count < 1)
        {
            error = Error.UserNotFound;
            return null;
        }

        if (players.Count > 1)
        {
            error = Error.TooManyUsers;
            return null;
        }

        return players[0];
    }


    private static ArcaeaSongs? QuerySongInfo(string? songname, string? songid, out Response? error)
    {
        error = null;

        if (!string.IsNullOrWhiteSpace(songid)) return ArcaeaSongs.GetById(songid);

        if (string.IsNullOrWhiteSpace(songname))
        {
            error = Error.InvalidSongNameorID;
            return null;
        }

        var ls = ArcaeaSongs.GetByAlias(songname);

        if (ls.Length < 1)
        {
            error = Error.SongNotFound;
            return null;
        }

        if (ls.Length > 1)
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
            if (friend.RecentScore[0].TimePlayed > 1609430400000)
                ArcaeaSongs.UpdateRating(friend.RecentScore[0]);

            Records.Insert(friend,friend.RecentScore[0]);
        }

        return friend;
    }


    private static (int, int) RangeConverter(string? start, string? end)
    {
        int upper, lower;
        if (start == null)
        {
            if (end == null) return (-1, -1);

            (lower, upper) = ConvertToArcaeaRange(end);
        }
        else
        {
            if (end is null)
                (lower, upper) = ConvertToArcaeaRange(start);

            else
            {
                (lower, _) = ConvertToArcaeaRange(start);
                (_, upper) = ConvertToArcaeaRange(end);
            }
        }

        return (lower, upper);
    }

    private static (int, int) ConvertToArcaeaRange(string rawdata) =>
        rawdata switch
        {
            "11"  => (110, 116),
            "10p" => (107, 109),
            "10"  => (100, 106),
            "9p"  => (97, 99),
            "9"   => (90, 96),
            "8"   => (80, 89),
            "7"   => (70, 79),
            "6"   => (60, 69),
            "5"   => (50, 59),
            "4"   => (40, 49),
            "3"   => (30, 39),
            "2"   => (20, 29),
            "1"   => (10, 19),
            _ => double.TryParse(rawdata, out var value)
                ? ((int)Math.Round(value * 10), (int)Math.Round(value * 10))
                : (-1, -1)
        };
}
