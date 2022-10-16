using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [AuthorizationCheck(Order = 0)]
    [PlayerInfoConverter(Order = 1)]
    [OverflowConverter(Order = 2)]
    [HttpGet("/botarcapi/user/best30")]
    public async Task<object> GetUserBest30(
        [BindNever] PlayerInfo player,
        [BindNever] int overflow,
        bool withrecent = false,
        bool withsonginfo = false)
    {
        try
        {
            TaskCompletionSource<(UserBest30Response? b30data, Response? error)>? task = UserBest30Concurrent.GetTask(player.Code);

            if (task is null)
            {
                UserBest30Concurrent.NewTask(player.Code);
                var (response, errorresp) = await QueryUserBest30(player);
                UserBest30Concurrent.SetResult(player.Code, (response, errorresp));
                return errorresp ?? GetResponse(response!, overflow, withrecent, withsonginfo);
            }
            else
            {
                var (response, errorresp) = await task.Task;
                return errorresp ?? GetResponse(response!, overflow, withrecent, withsonginfo);
            }
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
        if (response.Best30Overflow is not null)
            response.Best30Overflow = overflowCount == 0
                                          ? null!
                                          : response.Best30Overflow.Take(Math.Min(overflowCount, response.Best30Overflow.Count)).ToList();

        if (withsonginfo)
        {
            if (response.Best30List is not null) response.Best30Songinfo = response.Best30List.Select(i => ArcaeaCharts.QueryByRecord(i)!);

            if (response.Best30Overflow is not null)
                response.Best30OverflowSonginfo = response.Best30Overflow.Select(i => ArcaeaCharts.QueryByRecord(i)!);
        }

        if (withrecent)
        {
            if (response.AccountInfo.RecentScore is not null) response.RecentScore = response.AccountInfo.RecentScore.FirstOrDefault();
            if (withsonginfo) response.RecentSonginfo = ArcaeaCharts.QueryByRecord(response.RecentScore);
        }

        response.AccountInfo.RecentScore = null;

        return Success(response);
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
        catch (Exception ex)
        {
            Logger.ExceptionError(ex);
            return (null, Error.AddFriendFailed);
        }
        finally
        {
            AccountInfo.Recycle(account);
        }
    }
}
