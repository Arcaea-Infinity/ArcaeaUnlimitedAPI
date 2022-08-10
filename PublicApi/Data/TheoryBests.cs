using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [HttpGet("/botarcapi/data/theory")]
    public object GetTheoryBests(string? overflow, bool withrecent = false, bool withsonginfo = false,
                                 string? version = null)
    {
        // validate request arguments
        var overflowCount = 0;
        if (overflow is not null && (!int.TryParse(overflow, out overflowCount) || overflowCount is < 0 or > 10))
            return Error.InvalidRecentOrOverflowNumber;

        var count = overflowCount + 30;

        var verNum = double.NaN;
        if (version is not null && (!double.TryParse(version, out verNum) || verNum < 0)) return Error.InvalidVersion;

        var response = new UserBest30Response()
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

        foreach (var (sid, dif, rating) in ArcaeaCharts.SortedCharts)
        {
            if (results.Count >= count) break;

            var chart = ArcaeaCharts.QueryById(sid)![dif];
            if (double.IsNaN(verNum) || double.Parse(chart.Version) < verNum)
            {
                var result = new Records()
                             {
                                 BestClearType = 3,
                                 ClearType = 3,
                                 Difficulty = dif,
                                 Health = 100,
                                 MissCount = 0,
                                 Modifier = 0,
                                 NearCount = 0,
                                 PerfectCount = chart.Note,
                                 Rating = Utils.CalcSongRating(10000000, rating),
                                 Score = 10000000 + chart.Note,
                                 ShinyPerfectCount = chart.Note,
                                 TimePlayed = chart.Date,
                                 SongID = sid
                             };

                results.Add(result);
            }
        }

        response.Best30List = results.Take(30).ToList();

        response.Best30Overflow = overflowCount == 0
            ? null!
            : results.Skip(30).ToList();

        if (withsonginfo)
        {
            if (response.Best30List is not null)
                response.Best30Songinfo = response.Best30List.Select(i => ArcaeaCharts.QueryByRecord(i)!);

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
