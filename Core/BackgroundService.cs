using System.IO.Compression;
using System.Timers;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Json.Packlist;
using ArcaeaUnlimitedAPI.Json.Songlist;
using Downloader;
using Newtonsoft.Json;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.Core.Utils;
using Timer = System.Timers.Timer;

namespace ArcaeaUnlimitedAPI.Core;

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

        if (Config.OpenRegister == true) timer.Elapsed += (_, _) => ArcaeaFetch.RegisterTask();

        timer.Elapsed += ArcUpdate;
        timer.Elapsed += (_, _) => ++TimerCount;
        timer.AutoReset = true;
        timer.Start();
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

            if (!File.Exists(apkpth)) DownloadApk(info.Url);

            // not apk
            if (new FileInfo(apkpth).Length < 81920)
            {
                File.Delete(apkpth);
                return;
            }

            var apk = ZipFile.OpenRead(apkpth);

            var molnight = $"{Config.DataPath}/source/songs/melodyoflove_night.jpg";
            if (!File.Exists(molnight)) apk.GetEntry("assets/songs/dl_melodyoflove/base_night.jpg")!.ExtractToFile(molnight);

            foreach (var entry in apk.Entries)
            {
                if (entry.FullName.StartsWith("assets/char/") && entry.Name.EndsWith(".png"))
                {
                    var path = $"{Config.DataPath}/source/char/{entry.Name}";
                    entry.ExtractToFile(path, true);
                }
            }

            using var songlist = apk.GetEntry("assets/songs/songlist")!.Open();
            List<SongItem> list = JsonConvert.DeserializeObject<Songlist>(new StreamReader(songlist).ReadToEnd())!.Songs;

            foreach (var i in list)
            {
                var destdir = $"{Config.DataPath}/source/songs";
                var rawdir = $"assets/songs/{(i.NeedDownload ? "dl_" : string.Empty)}{i.Id}";

                for (var j = 0; j < i.Difficulties.Count; ++j)
                {
                    string pth, entry;

                    if (j == 2)
                    {
                        pth = $"{destdir}/{i.Id}.jpg";
                        entry = $"{rawdir}/base.jpg";
                    }
                    else if (i.Difficulties[j].JacketOverride)
                    {
                        pth = $"{destdir}/{i.Id}_{j}.jpg";
                        entry = $"{rawdir}/{j}.jpg";
                    }
                    else
                    {
                        continue;
                    }

                    if (!File.Exists(pth)) apk.GetEntry(entry)!.ExtractToFile(pth);
                }

                ArcaeaCharts.Insert(i);
            }

            using var packlist = apk.GetEntry("assets/songs/packlist")!.Open();
            Dictionary<string, PackItem> packs
                = JsonConvert.DeserializeObject<Packlist>(new StreamReader(packlist).ReadToEnd())!.Packs.ToDictionary(i => i.ID);

            foreach (var (_, packItem) in packs)
            {
                PackageInfo.Insert(new()
                                   {
                                       PackageID = packItem.ID,
                                       Name = string.IsNullOrWhiteSpace(packItem.PackParent)
                                                  ? packItem.NameLocalized.En
                                                  : packs[packItem.PackParent].NameLocalized.En
                                   });
            }

            Version = info.Version;
        }
        catch (Exception ex)
        {
            Logger.ExceptionError(ex);
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    private static void DownloadApk(string url)
    {
        using var downloader = new DownloadService(new()
                                                   {
                                                       BufferBlockSize = 8000,
                                                       ChunkCount = 16,
                                                       MaxTryAgainOnFailover = 10,
                                                       OnTheFlyDownload = false,
                                                       ParallelDownload = true,
                                                       Timeout = 5000
                                                   });

        downloader.DownloadFileTaskAsync(url, new DirectoryInfo($"{Config.DataPath}/update/")).Wait();
    }
}
