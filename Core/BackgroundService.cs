using System.Diagnostics;
using System.IO.Compression;
using System.Timers;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Json.Songlist;
using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.Core.Utils;
using Timer = System.Timers.Timer;

namespace ArcaeaUnlimitedAPI.Core;

public static class ConfigWatcher
{
    private static readonly int TimeoutMillis = 2000;
    private static readonly FileSystemWatcher Watcher = new(AppContext.BaseDirectory);

    static ConfigWatcher()
    {
        Watcher.NotifyFilter = NotifyFilters.LastWrite;
        Watcher.Filter = "config.json";
        Watcher.EnableRaisingEvents = true;

        System.Threading.Timer timer = new(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        Watcher.Changed += (_, _) => timer.Change(TimeoutMillis, Timeout.Infinite);
    }

    private static void OnTimer(object? _)
    {
        Log.FunctionLog("ConfigWatcher", "config changed.");
        Init();
    }
}

internal static class BackgroundService
{
    internal static ulong TimerCount;

    private static string? _version;

    private static volatile int _running;

    private static string Version
    {
        get => _version ??= File.ReadAllText($"{Config.DataPath}/arcversion");
        set
        {
            _version = value;
            File.WriteAllText($"{Config.DataPath}/arcversion", value);
        }
    }

    internal static void Init()
    {
        Timer timer = new(600000);

        if (Config.OpenRegister == true) timer.Elapsed += async (_, _) => await ArcaeaFetch.RegisterTask();

        timer.Elapsed += ArcUpdate;
        timer.Elapsed += TestNodes;
        timer.Elapsed += (_, _) => ++TimerCount;
        timer.AutoReset = true;
        timer.Start();
    }

    private static void TestNodes(object? source, ElapsedEventArgs? e)
    {
        if (TimerCount % 12 == 0) Config.Nodes.ForEach(TestNode);
    }

    private static void ArcUpdate(object? source, ElapsedEventArgs? e)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return;

        try
        {
            var info = GetLatestVersion().Result;
            if (info?.Url is null || Version == info.Version) return;

            if (Config.Appversion != info.Version) NeedUpdate = true;
            var dirname = info.Version;
            var apkpth = $"{Config.DataPath}/update/arcaea_{dirname}.apk";
            var dirpth = $"{Config.DataPath}/update/{dirname}dir/";

            try
            {
                if (!File.Exists(apkpth)) DownloadApk(info.Url);

                if (Directory.Exists(dirpth)) Directory.Delete(dirpth, true);
                Directory.CreateDirectory(dirpth);
                ZipFile.ExtractToDirectory(apkpth, dirpth);

                if (!File.Exists($"{Config.DataPath}/source/songs/melodyoflove_night.jpg"))
                    File.Move($"{dirpth}/assets/songs/dl_melodyoflove/base_night.jpg",
                              $"{Config.DataPath}/source/songs/melodyoflove_night.jpg");

                foreach (var file in new DirectoryInfo($"{dirpth}/assets/char/").GetFiles()
                                                                                .Where(file =>
                                                                                           !File
                                                                                               .Exists($"{Config.DataPath}/source/char/{file.Name}")))
                    file.MoveTo($"{Config.DataPath}/source/char/{file.Name}");

                var list = JsonConvert.DeserializeObject<Songlist>(File.ReadAllText($"{dirpth}/assets/songs/songlist"));

                if (list is not null)
                    foreach (var i in list.Songs)
                    {
                        var destdir = $"{Config.DataPath}/source/songs";
                        var rawdir = $"{dirpth}/assets/songs/{(i.NeedDownload ? "dl_" : "")}{i.Id}";

                        for (var j = 0; j < i.Difficulties.Count; ++j)
                            if (j == 2)
                            {
                                var pth = $"{destdir}/{i.Id}.jpg";
                                var rawpth = $"{rawdir}/base.jpg";
                                if (!File.Exists(pth) && File.Exists(rawpth)) File.Move(rawpth, pth);
                            }
                            else if (i.Difficulties[j].JacketOverride)
                            {
                                var pth = $"{destdir}/{i.Id}_{j}.jpg";
                                var rawpth = $"{rawdir}/{j}.jpg";
                                if (!File.Exists(pth) && File.Exists(rawpth)) File.Move(rawpth, pth);
                            }

                        ArcaeaSongs.Insert(i);
                        Thread.Sleep(300);
                        ArcaeaCharts.Insert(i);

                        if (i.Difficulties.Count == 4)
                        {
                            var song = ArcaeaSongs.GetById(i.Id);
                            if (song?.BynRating == -1)
                            {
                                song.BynRating = 0;
                                song.BynNote = 0;
                                song.Ratings[3] = song.BynRating;

                                DatabaseManager.Song.Update(song);
                            }
                        }
                    }

                AutoDecrypt(dirpth, info.Version);

                Version = info.Version;
                File.Delete(apkpth);
            }
            finally
            {
                Directory.Delete(dirpth, true);
            }
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    private static async void AutoDecrypt(string dirpth, string version)
    {
        try
        {
            var lib = await ArcaeaDecrypt.ReadLib($"{dirpth}/lib/arm64-v8a/libcocos2dcpp.so");
            var salt = ArcaeaDecrypt.GetSalt(lib);
            var cert = ArcaeaDecrypt.GetCert(lib);
            var entry = ArcaeaDecrypt.GetApiEntry(lib);

            await File.WriteAllBytesAsync($"{Config.DataPath}/cert-{version}.p12", cert);

            Config.ApiSalt = salt.ToList();
            Config.ApiEntry = entry;
            Config.CertFileName = $"cert-{version}.p12";
            Config.Appversion = version;

            Config.WriteConfig(false);

            var tmpfetch = new TestFetch();

            if (tmpfetch.Init(Config) && tmpfetch.TestLogin().Result) Config.WriteConfig(true);
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
        }
    }

    private static void DownloadApk(string url)
    {
        var psi = new ProcessStartInfo { FileName = "aria2c", Arguments = $"--dir={Config.DataPath}/update/ {url}" };
        using var p = Process.Start(psi);
        p?.WaitForExit();
        p?.Kill();
        p?.Close();
    }
}
