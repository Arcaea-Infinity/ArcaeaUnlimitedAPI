using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using Microsoft.AspNetCore.Mvc;
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

    private static bool ChartMissingCheck(ArcaeaSong song, sbyte difficulty) =>
        (difficulty == 3 && song.Count < 4) || (song.SongID == "lasteternity" && difficulty != 3);
    
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
            // Time > 2022/07/07 because 616 may change the calculation function of ptt
            if (friend.RecentScore[0].TimePlayed > 1657152000000) ArcaeaCharts.UpdateRating(friend.RecentScore[0]);

            Records.Insert(friend, friend.RecentScore[0]);
        }

        return friend;
    }
}
