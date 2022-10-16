using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;

namespace ArcaeaUnlimitedAPI.PublicApi;

// ReSharper disable InconsistentlySynchronizedField
internal class PollingBestsHelper
{
    private readonly AccountInfo _account;
    private readonly PriorityQueue<ArcaeaCharts, int> _failedlist = new();

    private readonly FriendsItem _friend;
    private readonly PriorityQueue<Records, double> _records = new();
    private readonly ConcurrentQueue<Task<Records?>> _tasks = new();

    private PollingBestsHelper(AccountInfo account, FriendsItem friend)
    {
        _account = account;
        _friend = friend;
    }

    internal static async Task<UserBest30Response?> GetResult(AccountInfo account, FriendsItem friend)
        => await new PollingBestsHelper(account, friend).PollingBests();

    private async Task<UserBest30Response?> PollingBests()
    {
        foreach (var item in ArcaeaCharts.SortedCharts)
        {
            var trypeek = _records.TryPeek(out _, out var minrating);

            // query charts until rating less than floor -2
            if (trypeek && _records.Count == 40 && item.Rating < (minrating - 2) * 10) break;

            _tasks.Enqueue(GetNewTask(item));

            if (_tasks.Count >= 5) await PollingRequests();
        }

        if (!_tasks.IsEmpty) await PollingRequests();

        await ClearFailedList();

        if (_failedlist.Count != 0) return null;

        Records[] ls = _records.UnorderedItems.Select(i => i.Element).OrderByDescending(i => i.Rating).ToArray();
        return new() { Best30List = ls.Take(30).ToList(), Best30Overflow = ls.Skip(30).ToList() };
    }

    private async Task PollingRequests()
    {
        // wait for responses
        Records?[] results = await Task.WhenAll(_tasks);
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

    private Task<Records?> GetNewTask(ArcaeaCharts chart)
        => Task.Run(() =>
        {
            var (success, result) = _account.FriendRank(chart).Result;

            // Check invalid response and add them into failed list
            if (!success || result is null)
            {
                _failedlist.Enqueue(chart, chart.Rating);
                return null;
            }

            foreach (var i in result)
            {
                i.Potential = _friend.Rating;
                i.Rating = Utils.CalcSongRating(i.Score, chart.Rating);
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
