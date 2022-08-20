namespace ArcaeaUnlimitedAPI.Core;

internal static class ConfigWatcher
{
    private const int TimeoutMillis = 2000;
    private static readonly FileSystemWatcher Watcher = new(AppContext.BaseDirectory);

    private static string _fileName = "";

    internal static void Init()
    {
        Watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
        Watcher.Filter = "*.json";
        Watcher.EnableRaisingEvents = true;

        Timer timer = new(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        Watcher.Changed += (_, e) =>
                           {
                               _fileName = e.Name!;
                               timer.Change(TimeoutMillis, Timeout.Infinite);
                           };
    }

    private static void OnTimer(object? _)
    {
        Log.FunctionLog("ConfigWatcher", "config changed.");
        GlobalConfig.Init(_fileName);
    }
}
