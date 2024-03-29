﻿using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public sealed partial class PublicApi
{
    [EnableCors]
    [OverflowConverter]
    [HttpGet("data/theory")]
    public JsonResult GetTheoryBests(
        [BindNever] int overflow,
        bool withrecent = false,
        bool withsonginfo = false,
        string? version = null)
    {
        var count = overflow + 30;

        var verNum = double.NaN;
        if (version is not null && (!double.TryParse(version, out verNum) || verNum < 1)) return Error.InvalidVersion;

        var response = new UserBest30Response
                       {
                           AccountInfo = new()
                                         {
                                             Character = 5,
                                             Code = "000000000",
                                             IsCharUncapped = true,
                                             IsCharUncappedOverride = true,
                                             JoinDate = 1487980800,
                                             UserID = 0,
                                             Name = $"Max Grades - v{version ?? GlobalConfig.Config.Appversion}"
                                         }
                       };

        var results = new List<Records>();

        // ReSharper disable once LoopCanBePartlyConvertedToQuery
        foreach (var chart in ArcaeaCharts.SortedCharts)
        {
            if (results.Count >= count) break;

            if (double.IsNaN(verNum) || double.Parse(chart.Version) <= verNum)
            {
                var result = new Records
                             {
                                 BestClearType = 3,
                                 ClearType = 3,
                                 Difficulty = chart.RatingClass,
                                 Health = 100,
                                 MissCount = 0,
                                 Modifier = 0,
                                 NearCount = 0,
                                 PerfectCount = chart.Note,
                                 Rating = Utils.CalcSongRating(10000000, chart.Rating),
                                 Score = 10000000 + chart.Note,
                                 ShinyPerfectCount = chart.Note,
                                 TimePlayed = chart.Date,
                                 SongID = chart.SongID
                             };

                results.Add(result);
            }
        }

        if (!results.Any()) return Error.InvalidVersion;

        response.Best30List = results.Take(30).ToList();
        response.Best30Overflow = overflow == 0 ? null! : results.Skip(30).ToList();
        response.Best30Avg = response.Best30List.Average(i => i.Rating);
        response.Recent10Avg = results.Take(10).Average(i => i.Rating);
        response.AccountInfo.Rating = (int)((response.Best30Avg * 3 + response.Recent10Avg) * 25);

        if (withsonginfo)
        {
            if (response.Best30List is not null) response.Best30Songinfo = response.Best30List.Select(i => ArcaeaCharts.QueryByRecord(i)!);

            if (response.Best30Overflow is not null)
                response.Best30OverflowSonginfo = response.Best30Overflow.Select(i => ArcaeaCharts.QueryByRecord(i)!);
        }

        if (withrecent)
        {
            response.RecentScore = response.Best30List?.FirstOrDefault();
            if (withsonginfo) response.RecentSonginfo = ArcaeaCharts.QueryByRecord(response.RecentScore);
        }

        return Success(response);
    }
}
