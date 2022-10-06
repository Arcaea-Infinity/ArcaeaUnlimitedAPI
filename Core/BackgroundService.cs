using System.IO.Compression;
using System.Timers;
using ArcaeaUnlimitedAPI.Beans;
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
            var dirpth = $"{Config.DataPath}/update/{dirname}dir/";

            try
            {
                if (!File.Exists(apkpth)) DownloadApk(info.Url);

                // not apk
                if (new FileInfo(apkpth).Length < 81920) File.Delete(apkpth);

                if (Directory.Exists(dirpth)) Directory.Delete(dirpth, true);
                Directory.CreateDirectory(dirpth);
                ZipFile.ExtractToDirectory(apkpth, dirpth);

                var molnight = $"{Config.DataPath}/source/songs/melodyoflove_night.jpg";
                if (!File.Exists(molnight)) File.Move($"{dirpth}/assets/songs/dl_melodyoflove/base_night.jpg", molnight);

                foreach (var file in new DirectoryInfo($"{dirpth}/assets/char/").GetFiles())
                {
                    var name = $"{Config.DataPath}/source/char/{file.Name}";
                    if (!File.Exists(name)) file.MoveTo(name);
                }

                var list = JsonConvert.DeserializeObject<Songlist>(File.ReadAllText($"{dirpth}/assets/songs/songlist"));

                if (list is not null)
                    foreach (var i in list.Songs)
                    {
                        var destdir = $"{Config.DataPath}/source/songs";
                        var rawdir = $"{dirpth}/assets/songs/{(i.NeedDownload ? "dl_" : string.Empty)}{i.Id}";

                        for (var j = 0; j < i.Difficulties.Count; ++j)
                        {
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
                        }

                        ArcaeaCharts.Insert(i);
                        Thread.Sleep(300);
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

    private static void AutoDecrypt(string dirpth, string version)
    {
        try
        {
            var decrypt = new ArcaeaDecrypt();

            decrypt.ReadLib($"{dirpth}/lib/arm64-v8a/libcocos2dcpp.so");

            File.WriteAllBytes($"{Config.DataPath}/cert-{version}.p12", decrypt.GetCert());

            Config.ApiSalt = decrypt.GetSalt();
            Config.ApiEntry = decrypt.GetApiEntry();
            Config.CertFileName = $"cert-{version}.p12";
            Config.Appversion = version;
            Config.WriteConfig();

            NeedUpdate = false;
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
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
