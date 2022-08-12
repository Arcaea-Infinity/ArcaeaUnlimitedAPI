using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.PublicApi.Params;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.Utils;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [UpdateCheck]
    [UserAgentAuth]
    [HttpGet("/botarcapi/user/best")]
    public async Task<object> GetUserBest([FromQuery] PlayerInfoParams playerInfo, 
                                          [FromQuery] SongInfoParams songInfo,
                                          [FromQuery] DifficultyParams difficultyInfo,
                                          bool withrecent = false,
                                          bool withsonginfo = false)
    {
        // validate request arguments

        var difficultyNum = difficultyInfo.Validate(out var difficultyerror);
        if (difficultyerror is not null) return difficultyerror;

        var player = playerInfo.Validate(out var playererror);
        if (player is null) return playererror ?? Error.UserNotFound;

        var song = songInfo.Validate(out var songerror);
        if (song is null) return songerror ?? Error.InvalidSongNameorID;

        // validate exist chart 
        if (ChartMissingCheck(song, difficultyNum)) return Error.NoThisLevel;
        var chart = song[difficultyNum];

        var key = (player.Code, chart.SongID, chart.RatingClass);

        try
        {
            var task = UserBestConcurrent.GetTask(key);

            if (task is null)
            {
                UserBestConcurrent.NewTask(key);
                var (response, errorresp) = await QueryUserBest(player, chart, difficultyNum);
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
            UserBestConcurrent.GotResultCallBack(key);
        }
    }

    private static Response GetResponse(UserBestResponse response, bool withrecent, bool withsonginfo,
                                        ArcaeaCharts chart)
    {
        if (withsonginfo)
            // add song info
            response.Songinfo = new[] { chart };

        if (withrecent)
        {
            if (response.AccountInfo.RecentScore is not null)
                response.RecentScore = response.AccountInfo.RecentScore.FirstOrDefault();
            if (withsonginfo) response.RecentSonginfo = ArcaeaCharts.QueryByRecord(response.RecentScore);
        }

        response.AccountInfo.RecentScore = null!;

        return Success(response);
    }

    private static async Task<(UserBestResponse? response, Response? error)> QueryUserBest(
        PlayerInfo player, ArcaeaCharts chart, sbyte difficulty)
    {
        AccountInfo? account = null;

        try
        {
            account = await AccountInfo.Alloc();
            if (account is null) return (null, Error.AllocateAccountFailed);
            var friend = RecordPlayers(account, player, out var recorderror);
            if (friend is null) return (null, recorderror!);

            // get rank result
            var (success, friendRank) = await account.FriendRank(chart.SongID, difficulty);
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
