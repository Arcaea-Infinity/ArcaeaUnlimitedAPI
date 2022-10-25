using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.Core.Utils;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [AuthorizationCheck(Order = 0)]
    [PlayerInfoConverter(Order = 1)]
    [SongInfoConverter(Order = 2)]
    [DifficultyConverter(false, Order = 3)]
    [ChartConverter(Order = 4)]
    [HttpGet("/botarcapi/user/best")]
    public async Task<object> GetUserBest(
        [BindNever] [FromQuery] PlayerInfo player,
        [BindNever] [FromQuery] ArcaeaCharts chart,
        [BindNever] string currentTokenID,
        bool withrecent = false,
        bool withsonginfo = false)
    {
        var key = (player.Code, chart.SongID, chart.RatingClass);

        try
        {
            TaskCompletionSource<(UserBestResponse? bestdata, Response? error)>? task = UserBestConcurrent.GetTask(key);
            UserBestResponse? response;
            Response? errorresp;

            if (task is null)
            {
                UserBestConcurrent.NewTask(key);
                (response, errorresp) = await QueryUserBest(player, chart, currentTokenID);
                UserBestConcurrent.SetResult(key, (response, errorresp));
            }
            else
            {
                (response, errorresp) = await task.Task;
            }

            return errorresp ?? GetResponse(response!, withrecent, withsonginfo, chart);
        }
        finally
        {
            UserBestConcurrent.CallBack(key);
        }
    }

    private static Response GetResponse(
        UserBestResponse response,
        bool withrecent,
        bool withsonginfo,
        ArcaeaCharts chart)
    {
        UserBestResponse ret = new() { AccountInfo = response.AccountInfo, Record = response.Record };

        if (withsonginfo)
            // add song info
            ret.Songinfo = new[] { chart };

        if (withrecent)
        {
            if (response.AccountInfo.RecentScore is not null) ret.RecentScore = response.AccountInfo.RecentScore.FirstOrDefault();
            if (withsonginfo) ret.RecentSonginfo = ArcaeaCharts.QueryByRecord(ret.RecentScore);
        }

        ret.AccountInfo.RecentScore = null!;

        return Success(ret);
    }

    private static async Task<(UserBestResponse? response, Response? error)> QueryUserBest(PlayerInfo player, ArcaeaCharts chart, string tokenid)
    {
        AccountInfo? account = null;

        try
        {
            account = await AccountInfo.Alloc(tokenid);
            if (account is null) return (null, Error.AllocateAccountFailed);

            var friend = RecordPlayers(account, player, out var recorderror);
            if (friend is null) return (null, recorderror!);

            // get rank result
            var (success, friendRank) = await account.FriendRank(chart);
            if (!success || friendRank is null || friendRank.Count == 0) return (null, Error.NotPlayedYet);
            foreach (var record in friendRank)
            {
                record.Potential = player.Potential;
                DatabaseManager.Bests.InsertOrReplace(record);
            }

            // calculate song rating
            var rank = friendRank[0];
            rank.Rating = CalcSongRating(rank.Score, chart.Rating);

            rank.UserID = null!;

            return (new() { AccountInfo = friend, Record = rank }, null);
        }
        finally
        {
            AccountInfo.Recycle(account);
        }
    }
}
