using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Core;

namespace ArcaeaUnlimitedAPI.PublicApi;

// ReSharper disable InconsistentlySynchronizedField
internal class PollingBestsHelper
{
    private readonly PriorityQueue<(string songID, int songDif, int songRating), int> _failedlist = new();
    private readonly PriorityQueue<Records, double> _records = new();
    private readonly ConcurrentQueue<Task<Records?>> _tasks = new();

    private readonly int _friendUserID;
    private readonly AccountInfo _account;

    private PollingBestsHelper(AccountInfo account, int friendUserID)
    {
        _account = account;
        _friendUserID = friendUserID;
    }

    internal static async Task<UserBest30Response?> GetResult(AccountInfo account, int userID) =>
        await new PollingBestsHelper(account, userID).PollingBests();

    private async Task<UserBest30Response?> PollingBests()
    {
        foreach (var item in ArcaeaSongs.SortByRating.ToArray())
        {
            var trypeek = _records.TryPeek(out _, out var minrating);

            // query charts until rating less than floor -2
            if (trypeek && _records.Count == 40 && item.rating < (minrating - 2) * 10) break;

            _tasks.Enqueue(GetNewTask(item));

            if (_tasks.Count >= 5) await PollingRequests();
        }

        if (_tasks.Count > 0) await PollingRequests();

        await ClearFailedList();

        if (_failedlist.Count != 0)
            return null;

        var ls = _records.UnorderedItems.Select(i => i.Element).OrderByDescending(i => i.Rating).ToArray();
        return new() { Best30List = ls.Take(30).ToList(), Best30Overflow = ls.Skip(30).ToList() };
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

        if(oricount > 10) return;
        
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

    private Task<Records?> GetNewTask((string songID, int songDif, int songRating) tuple)
    {
        return Task.Run(() =>
                        {
                            var (songID, songDif, songRating) = tuple;

                            var (success, result) = _account.FriendRank(songID, songDif).Result;

                            // Check invalid response and add them into failed list
                            if (!success || result is null)
                            {
                                _failedlist.Enqueue(tuple, songRating);
                                return null;
                            }

                            var record = result.FirstOrDefault(i => i.UserID == _friendUserID);

                            if (record is null) return null;

                            //for json
                            record.UserID = null!;
                            record.Rating = Utils.CalcSongRating(record.Score, songRating);

                            return record;
                        });
    }

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
        else if (minrating < record.Rating)
            lock (_records)
            {
                _records.EnqueueDequeue(record, record.Rating);
                return;
            }
    }
}
