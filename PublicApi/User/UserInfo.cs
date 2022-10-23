﻿using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [AuthorizationCheck(Order = 0)]
    [PlayerInfoConverter(Order = 1)]
    [RecentConverter(Order = 2)]
    [HttpGet("/botarcapi/user/info")]
    public async Task<object> GetUserInfo([BindNever] PlayerInfo player, [BindNever] int recent, bool withsonginfo = false)
    {
        try
        {
            TaskCompletionSource<(UserInfoResponse? infodata, Response? error)>? task = UserInfoConcurrent.GetTask(player.Code);
            UserInfoResponse? response;
            Response? errorresp;

            if (task is null)
            {
                UserInfoConcurrent.NewTask(player.Code);
                (response, errorresp) = await QueryUserInfo(player);
                UserInfoConcurrent.SetResult(player.Code, (response, errorresp));
            }
            else
            {
                (response, errorresp) = await task.Task;
            }

            return errorresp ?? GetResponse(response!, recent, withsonginfo);
        }
        finally
        {
            UserInfoConcurrent.CallBack(player.Code);
        }
    }

    private static Response GetResponse(UserInfoResponse response, int recent, bool withsonginfo)
    {
        UserInfoResponse ret = new()
        {
            AccountInfo = response.AccountInfo,
            RecentScore = recent switch
                          {
                              0   => null,
                              > 1 => Records.Query(response.AccountInfo.UserID, recent),
                              1   => response.RecentScore?.ToList(),
                              _   => null
                          }
        };
        
        if (ret.RecentScore?.Count > 0)
        {
            foreach (var record in ret.RecentScore) record.UserID = null!;
            if (withsonginfo) ret.Songinfo = ret.RecentScore.Select(i => ArcaeaCharts.QueryByRecord(i)!);
        }
        
        ret.AccountInfo.RecentScore = null!;

        return Success(ret);
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
        finally
        {
            AccountInfo.Recycle(account);
        }
    }
}
