using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.PublicApi.Params;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [UpdateCheck]
    [UserAgentAuth]
    [HttpGet("/botarcapi/user/info")]
    public async Task<object> GetUserInfo([FromQuery] PlayerInfoParams playerInfo,
                                          [FromQuery] RecentParams recentInfo,
                                          bool withsonginfo = false)
    {
        // validate request arguments

        var player = playerInfo.Validate(out var playererror);
        if (player is null) return playererror ?? Error.UserNotFound;

        var recentCount = recentInfo.Validate(out var recenterror);
        if (recenterror is not null) return recenterror;

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
