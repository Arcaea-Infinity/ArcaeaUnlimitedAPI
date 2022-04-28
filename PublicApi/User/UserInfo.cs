using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [HttpGet("/botarcapi/user/info")]
    public async Task<object> GetUserInfo(string? user, string? usercode, string? recent, bool withsonginfo = false)
    {
        if (!UserAgentCheck()) return NotFound(null);
        if (NeedUpdate) return Error.NeedUpdate;

        // validate request arguments
        var recentCount = 1;
        if (recent is not null)
            if (!int.TryParse(recent, out recentCount) || recentCount is < 0 or > 7)
                return Error.InvalidRecentOrOverflowNumber;

        var player = QueryPlayerInfo(user, usercode, out var playererror);
        if (player is null) return playererror!;

        try
        {
            var task = UserInfoConcurrent.GetTask(player.Code);

            if (task is null)
            {
                UserInfoConcurrent.NewTask(player.Code);
                var (response, errorresp) = await QueryUserInfo(player);
                UserInfoConcurrent.SetResult(player.Code, (response, errorresp));
                return errorresp ?? GetResponse(response!, recentCount, withsonginfo);
            }
            else
            {
                var (response, errorresp) = await task.Task;
                return errorresp ?? GetResponse(response!, recentCount, withsonginfo);
            }
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
            return Error.InternalErrorOccurred;
        }
        finally
        {
            UserInfoConcurrent.GotResultCallBack(player.Code);
        }
    }

    private static Response GetResponse(UserInfoResponse response, int recent, bool withsonginfo)
    {
        response.AccountInfo.RecentScore = null!;

        response.RecentScore = recent switch
                               {
                                   0   => null,
                                   > 1 => Records.Query(response.AccountInfo.UserID, recent),
                                   _   => response.RecentScore
                               };

        if (response.RecentScore?.Count > 0)
        {
            foreach (var record in response.RecentScore) record.UserID = null!;

            if (withsonginfo) response.Songinfo = response.RecentScore.Select(i => ArcaeaCharts.QueryByRecord(i)!);
        }

        return Success(response);
    }

    private static async Task<(UserInfoResponse? response, Response? error)> QueryUserInfo(PlayerInfo player)
    {
        AccountInfo? account = null;

        try
        {
            account = await AccountInfo.Alloc();
            if (account is null) return (null, Error.AllocateAccountFailed);
            var friend = RecordPlayers(account, player, out var recorderror);
            if (friend is null) return (null, recorderror!);

            return (new() { AccountInfo = friend, RecentScore = friend.RecentScore }, null);
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
