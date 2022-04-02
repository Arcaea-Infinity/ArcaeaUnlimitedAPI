using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.Core.Utils;
using static ArcaeaUnlimitedAPI.PublicApi.Response;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/user/best")]
    public async Task<object> GetUserBest(string? user, string? usercode, string? songname, string? songid,
                                          string? difficulty, bool withrecent = false, bool withsonginfo = false)
    {
        if (!UserAgentCheck()) return NotFound(null);
        if (NeedUpdate) return Error.NeedUpdate;

        // validate request arguments
        if (!DifficultyInfo.TryParse(difficulty, out var difficultyNum)) return Error.InvalidDifficulty;

        var song = QuerySongInfo(songname, songid, out var songerror);
        if (song is null) return songerror ?? Error.InvalidSongNameorID;

        // check for beyond is existed
        if (difficultyNum == 3 && song.BynRating == -1) return Error.NoBeyondLevel;

        var player = QueryPlayerInfo(user, usercode, out var playererror);
        if (player is null) return playererror!;

        try
        {
            var task = UserBestConcurrent.GetTask(player.Code);

            if (task is null)
            {
                UserBestConcurrent.NewTask(player.Code);
                var (response, errorresp) = await QueryUserBest(player, song, difficultyNum);
                UserBestConcurrent.SetResult(player.Code, (response, errorresp));
                return errorresp ?? GetResponse(response!, withrecent, withsonginfo);
            }
            else
            {
                var (response, errorresp) = await task.Task;
                return errorresp ?? GetResponse(response!, withrecent, withsonginfo);
            }
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
            return Error.InternalErrorOccurred;
        }
        finally
        {
            UserBestConcurrent.GotResultCallBack(player.Code);
        }
    }

    private Response GetResponse(UserBestResponse response, bool withrecent, bool withsonginfo)
    {
        if (withsonginfo)
            // add song info
            response.Songinfo = new[] { ArcaeaSongs.GetById(response.Record.SongID)!.ToJson() };

        if (withrecent)
        {
            if (response.AccountInfo.RecentScore is not null)
                response.RecentScore = response.AccountInfo.RecentScore.FirstOrDefault();
            if (withsonginfo) response.RecentSonginfo = ArcaeaSongs.GetById(response.RecentScore?.SongID)?.ToJson();
        }

        response.AccountInfo.RecentScore = null!;

        return Success(response);
    }

    private static async Task<(UserBestResponse? response, Response? error)> QueryUserBest(
        PlayerInfo player, ArcaeaSongs song, sbyte difficulty)
    {
        AccountInfo? account = null;

        try
        {
            account = await AccountInfo.Alloc();
            if (account is null) return (null, Error.AllocateAccountFailed);
            var friend = RecordPlayers(account, player, out var recorderror);
            if (friend is null) return (null, recorderror!);

            // get rank result
            var (success, friendRank) = await account.FriendRank(song.SongId, difficulty);
            if (!success || friendRank is null || friendRank.Count == 0) return (null, Error.NotPlayedYet);
            foreach (var record in friendRank)
            {
                record.Potential = player.Potential;
                DatabaseManager.Bests.InsertOrReplace(record);
            }
            
            // calculate song rating
            var rank = friendRank![0];
            rank.Rating = CalcSongRating(rank.Score, song.Ratings[rank.Difficulty]);

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
