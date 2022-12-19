using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [APIStatusCheck(Order = 0)]
    [AuthorizationCheck(Order = 1)]
    [PlayerInfoConverter(Order = 2)]
    [OverflowConverter(Order = 3)]
    [HttpGet("user/best30")]
    public async Task<object> GetUserBest30(
        [BindNever] PlayerInfo player,
        [BindNever] int overflow,
        bool withrecent = false,
        bool withsonginfo = false)
    {
        try
        {
            TaskCompletionSource<(UserBest30Response? b30data, Response? error)>? task = UserBest30Concurrent.GetTask(player.Code);
            UserBest30Response? response;
            Response? errorresp;

            if (task is null)
            {
                UserBest30Concurrent.NewTask(player.Code);
                (response, errorresp) = await QueryUserBest30(player);
                UserBest30Concurrent.SetResult(player.Code, (response, errorresp));
            }
            else
            {
                (response, errorresp) = await task.Task;
            }

            return errorresp ?? GetResponse(response!, overflow, withrecent, withsonginfo);
        }
        finally
        {
            UserBest30Concurrent.CallBack(player.Code);
        }
    }

    private static Response GetResponse(
        UserBest30Response response,
        int overflowCount,
        bool withrecent,
        bool withsonginfo)
    {
        UserBest30Response ret = new()
                                 {
                                     AccountInfo = response.AccountInfo,
                                     Best30Avg = response.Best30Avg,
                                     Recent10Avg = response.Recent10Avg,
                                     Best30List = response.Best30List,
                                 };
        
        if (response.Best30Overflow is not null)
            ret.Best30Overflow = overflowCount == 0
                                     ? null!
                                     : response.Best30Overflow.Take(Math.Min(overflowCount, response.Best30Overflow.Count)).ToList();

        if (withsonginfo)
        {
            if (ret.Best30List is not null) ret.Best30Songinfo = ret.Best30List.Select(i => ArcaeaCharts.QueryByRecord(i)!);

            if (ret.Best30Overflow is not null)
                ret.Best30OverflowSonginfo = ret.Best30Overflow.Select(i => ArcaeaCharts.QueryByRecord(i)!);
        }

        if (withrecent)
        {
            if (response.AccountInfo.RecentScore is not null) ret.RecentScore = response.AccountInfo.RecentScore.FirstOrDefault();
            if (withsonginfo) ret.RecentSonginfo = ArcaeaCharts.QueryByRecord(ret.RecentScore);
        }

        ret.AccountInfo.RecentScore = null;

        return Success(ret);
    }

    private static async Task<(UserBest30Response? response, Response? error)> QueryUserBest30(PlayerInfo player)
    {
        AccountInfo? account = null;
        try
        {
            account = await AccountInfo.Alloc();
            if (account is null) return (null, Error.AllocateAccountFailed);

            var friend = RecordPlayers(account, player, out var recorderror);
            if (friend is null) return (null, recorderror!);
            if (friend.Rating is >= 0 and < 700) return (null, Error.BelowTheThreshold);
            if (friend.RecentScore?.Any() != true) return (null, Error.NotPlayedYet);

            // read best30 cache from database
            var best30Cache = UserBest30Response.GetById(friend.UserID);

            // confirm update cache is needed by compare last played time
            if (best30Cache is null || best30Cache.LastPlayed != friend.RecentScore[0].TimePlayed)
            {
                // check shadow ban
                {
                    var (success, friendRank) = await account.FriendRank(ArcaeaCharts.QueryByRecord(friend.RecentScore[0])!);
                    if (!success || friendRank is null || friendRank.Count == 0) return (null, Error.Shadowbanned);

                    foreach (var record in friendRank)
                    {
                        record.Potential = player.Potential;
                        DatabaseManager.Bests.InsertOrReplace(record);
                    }
                }

                best30Cache = await PollingBestsHelper.GetResult(account, friend);

                if (best30Cache is null || best30Cache.Best30List is null || best30Cache.Best30List.Count == 0)
                    return (null, Error.QueryingB30Failed);

                best30Cache.Best30Avg = best30Cache.Best30List.Average(i => i.Rating);
                best30Cache.Recent10Avg = friend.Rating < 0 ? 0 : Math.Max(0, (double)friend.Rating / 100 * 4 - best30Cache.Best30Avg * 3);

                best30Cache.UserID = friend.UserID;
                best30Cache.LastPlayed = friend.RecentScore[0].TimePlayed;

                UserBest30Response.Update(best30Cache);
            }

            best30Cache.AccountInfo = friend;
            return (best30Cache, null);
        }
        finally
        {
            AccountInfo.Recycle(account);
        }
    }
}
