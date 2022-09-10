using System.Collections.Concurrent;

namespace ArcaeaUnlimitedAPI.PublicApi;

internal class ConcurrentApiRequest<T, TU> where T : notnull
{
    private readonly ConcurrentDictionary<T, TaskCompletionSource<TU>> _connPending = new();

    private readonly ConcurrentDictionary<T, uint> _connPendingCounter = new();

    internal void NewTask(T key)
    {
        if (_connPending.ContainsKey(key) && _connPendingCounter.ContainsKey(key)) return;

        var task = new TaskCompletionSource<TU>();

        // Put async task
        _connPending.TryAdd(key, task);
        _connPendingCounter.TryAdd(key, 1);
    }

    internal TaskCompletionSource<TU>? GetTask(T key)
    {
        if (_connPending.TryGetValue(key, out TaskCompletionSource<TU>? task))
        {
            ++_connPendingCounter[key];
            return task;
        }

        return null;
    }

    internal void CallBack(T key)
    {
        if (_connPendingCounter.ContainsKey(key))
        {
            --_connPendingCounter[key];

            if (_connPendingCounter[key] == 0)
            {
                _connPending.Remove(key, out _);
                _connPendingCounter.Remove(key, out _);
            }
        }
    }

    internal void SetResult(T key, TU result)
    {
        if (_connPending.TryGetValue(key, out TaskCompletionSource<TU>? task)) task.SetResult(result);
    }
}
