using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.Core.Utils;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [AuthorizationCheck(Order = 0)]
    [PlayerInfoConverter(Order = 1)]
    [SongInfoConverter(Order = 2)]
    [DifficultyConverter(Order = 3)]
    [ChartConverter(Order = 4)]
    [HttpGet("/botarcapi/user/best")]
    public async Task<object> GetUserBest(
        [BindNever] [FromQuery] PlayerInfo player,
        [BindNever] [FromQuery] ArcaeaCharts chart,
        bool withrecent = false,
        bool withsonginfo = false)
    {
        var key = (player.Code, chart.SongID, chart.RatingClass);

        try
        {
            TaskCompletionSource<(UserBestResponse? bestdata, Response? error)>? task = UserBestConcurrent.GetTask(key);

            if (task is null)
            {
                UserBestConcurrent.NewTask(key);
                var (response, errorresp) = await QueryUserBest(player, chart);
                UserBestConcurrent.SetResult(key, (response, errorresp));
                return errorresp ?? GetResponse(response!, withrecent, withsonginfo, chart);
            }
            else
            {
                var (response, errorresp) = await task.Task;
                return errorresp ?? GetResponse(response!, withrecent, withsonginfo, chart);
            }
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
            return Error.InternalErrorOccurred;
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
        if (withsonginfo)
            // add song info
            response.Songinfo = new[] { chart };

        if (withrecent)
        {
            if (response.AccountInfo.RecentScore is not null) response.RecentScore = response.AccountInfo.RecentScore.FirstOrDefault();
            if (withsonginfo) response.RecentSonginfo = ArcaeaCharts.QueryByRecord(response.RecentScore);
        }

        response.AccountInfo.RecentScore = null!;

        return Success(response);
    }

    private static async Task<(UserBestResponse? response, Response? error)> QueryUserBest(PlayerInfo player, ArcaeaCharts chart)
    {
        AccountInfo? account = null;

        try
        {
            account = await AccountInfo.Alloc();
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
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
            return (null, Error.AddFriendFailed);
        }
        finally
        {
            AccountInfo.Recycle(account);
        }
    }
}
