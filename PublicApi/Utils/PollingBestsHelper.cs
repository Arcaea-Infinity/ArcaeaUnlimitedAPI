using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

namespace ArcaeaUnlimitedAPI.PublicApi;

// ReSharper disable InconsistentlySynchronizedField
internal class PollingBestsHelper
{
    private readonly AccountInfo _account;
    private readonly PriorityQueue<(string songID, int songDif, int songRating), int> _failedlist = new();

    private readonly FriendsItem _friend;
    private readonly PriorityQueue<Records, double> _records = new();
    private readonly ConcurrentQueue<Task<Records?>> _tasks = new();

    private PollingBestsHelper(AccountInfo account, FriendsItem friend)
    {
        _account = account;
        _friend = friend;
    }

    internal static async Task<UserBest30Response?> GetResult(AccountInfo account, FriendsItem friend) =>
        await new PollingBestsHelper(account, friend).PollingBests();

    private async Task<UserBest30Response?> PollingBests()
    {
        foreach (var item in ArcaeaCharts.SortedCharts)
        {
            var trypeek = _records.TryPeek(out _, out var minrating);

            // query charts until rating less than floor -2
            if (trypeek && _records.Count == 40 && item.rating < (minrating - 2) * 10) break;

            _tasks.Enqueue(GetNewTask(item));

            if (_tasks.Count >= 5) await PollingRequests();
        }

        if (!_tasks.IsEmpty) await PollingRequests();

        await ClearFailedList();

        if (_failedlist.Count != 0) return null;

        var ls = _records.UnorderedItems.Select(i => i.Element).OrderByDescending(i => i.Rating).ToArray();
        return new() { Best30List = ls.Take(30).ToList(), Best30Overflow = ls.Skip(30).ToList() };
    }

    private static double GetMaxDateByVersion(string? version)
    {
        if (version is null) return double.PositiveInfinity;
        var allVerSongs = ArcaeaCharts.GetByVersion(version);
        return allVerSongs is null ? double.NegativeInfinity : allVerSongs.Max(f => f[0].Date);
    }
    
    public static (string, UserBest30Response?) GetTheoryBest30(int overflow = 0, bool withrecent = false, 
        bool withsonginfo = false, string? queryVersion = null)
    {
        double b30Total = .0, r10Total = .0;
        var fCount = 0;
        var queryLatestDate = GetMaxDateByVersion(queryVersion);
        if (double.IsNegativeInfinity(queryLatestDate)) return ("Invalid parameter: version", null);  // check version
        
        var showName = double.IsPositiveInfinity(queryLatestDate) ? GlobalConfig.Config.Appversion : queryVersion;
        
        var retMaxB30 = new UserBest30Response()
        {
            AccountInfo = new FriendsItem()
            {
                Character = 5, Code = "000000000", IsCharUncapped = true, IsCharUncappedOverride = true, IsMutual = false,
                IsSkillSealed = false, JoinDate = 1487980800, Name = $"Max Grades - v{showName}", 
                UserID = -1
            },
            Best30List = new List<Records>()
        };
        if (overflow > 0) retMaxB30.Best30Overflow = new List<Records>();

        foreach (var item in ArcaeaCharts.SortedCharts)
        {
            var songsData = ArcaeaCharts.QueryById(item.sid);
            if (songsData is null) throw new Exception($"\"{item.sid}\" was not found in the database.");
            var songData = songsData[item.dif];
            
            if (songData.Date > queryLatestDate) continue;
            
            var addRecord = new Records()
            {
                BestClearType = 3, ClearType = 3, Difficulty = item.dif, Health = 100, MissCount = 0, Modifier = 0,
                NearCount = 0, PerfectCount = songData.Note, Potential = 0, Rating = (double)(item.rating + 20) / 10,
                Score = 10000000 + songData.Note, TimePlayed = songData.Date,
                ShinyPerfectCount = songData.Note,
                SongID = item.sid
            };

            if (fCount <= 29)
            {
                if (fCount <= 9) r10Total += item.rating + 20;
                b30Total += item.rating + 20;
                retMaxB30.Best30List.Add(addRecord);
            } 
            else if (fCount <= 29 + overflow && retMaxB30.Best30Overflow is not null)
            {
                retMaxB30.Best30Overflow.Add(addRecord);
            }
            else
            {
                break;
            }
            fCount++;
        }
        
        if (withsonginfo)
        {
            retMaxB30.Best30Songinfo = retMaxB30.Best30List.Select(i => ArcaeaCharts.QueryByRecord(i)!);
            if (retMaxB30.Best30Overflow is not null) 
                retMaxB30.Best30OverflowSonginfo = retMaxB30.Best30Overflow.Select(i => ArcaeaCharts.QueryByRecord(i)!);
        }
        
        if (withrecent)
        {
            retMaxB30.RecentScore = retMaxB30.Best30List[0];
            if (withsonginfo) retMaxB30.RecentSonginfo = ArcaeaCharts.QueryByRecord(retMaxB30.RecentScore);
        }
        
        retMaxB30.AccountInfo.Rating = (int)(b30Total + r10Total) / 4;
        retMaxB30.Best30Avg = b30Total / 300;
        retMaxB30.Recent10Avg = r10Total / 100;
        return ("success", retMaxB30);
    }

    private async Task PollingRequests()
    {
        // wait for responses
        var results = await Task.WhenAll(_tasks);
        _tasks.Clear();

        InsertRecords(results);
    }

    private async Task ClearFailedList()
    {
        var oricount = _failedlist.Count;
        var index = 0;

        if (oricount > 10) return;

        while (_failedlist.TryDequeue(out var item, out var rating))
        {
            if (_records.TryPeek(out _, out var minrating) && rating < (minrating - 2) * 10)
            {
                _failedlist.Clear();
                return;
            }

            InsertRecord(await GetNewTask(item));

            if (index++ > oricount + 5) return;
        }
    }

    private Task<Records?> GetNewTask((string songID, int songDif, int songRating) tuple) =>
        Task.Run(() =>
                 {
                     var (songID, songDif, songRating) = tuple;

                     var (success, result) = _account.FriendRank(songID, songDif).Result;


                     // Check invalid response and add them into failed list
                     if (!success || result is null)
                     {
                         _failedlist.Enqueue(tuple, songRating);
                         return null;
                     }

                     foreach (var i in result)
                     {
                         i.Potential = _friend.Rating;
                         i.Rating = Utils.CalcSongRating(i.Score, songRating);
                         DatabaseManager.Bests.InsertOrReplace(i);
                     }

                     var record = result.FirstOrDefault(i => i.UserID == _friend.UserID);

                     if (record is null) return null;

                     //for json
                     record.UserID = null!;

                     return record;
                 });

    private void InsertRecords(Records?[] results)
    {
        foreach (var record in results) InsertRecord(record);
    }

    private void InsertRecord(Records? record)
    {
        if (record == null) return;

        if (_records.Count < 40 || !_records.TryPeek(out _, out var minrating))
            lock (_records)
            {
                _records.Enqueue(record, record.Rating);
                return;
            }

        if (minrating < record.Rating)
            lock (_records)
            {
                _records.EnqueueDequeue(record, record.Rating);
                return;
            }
    }
}
