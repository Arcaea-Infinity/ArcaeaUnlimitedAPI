using System.IO.Compression;
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

    private static volatile int _running;

    private static string? _version;
    private static Timer _timer = null!;
    private static Timer _configTimer = null!;

    internal static string Version
    {
        get => _version ??= File.ReadAllText($"{Config.DataPath}/arcversion");
        private set
        {
            _version = value;
            File.WriteAllText($"{Config.DataPath}/arcversion", value);
        }
    }

    internal static void Init()
    {
        _timer = new(600000);

        if (Config.OpenRegister == true) _timer.Elapsed += (_, _) => ArcaeaFetch.RegisterTask();

        _timer.Elapsed += (_, _) =>
        {
            ArcUpdate();
            ++TimerCount;
        };

        _timer.Start();
    }

    internal static void ArcUpdate()
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) != 0) return;

        Console.WriteLine("Checking for updates......it will take a long time.");

        ZipArchive? apk = null;

        try
        {
            var info = GetLatestVersion().Result;
            if (info?.Url is null || Version == info.Version) return;

            if (Config.Appversion != info.Version)
            {
                NeedUpdate = true;
                Console.WriteLine($"Current config version: {Config.Appversion}]\nNewest version: {info.Version}");
            }

            var version = info.Version;
            var apkpth = Path.Combine(Config.DataPath, "update", $"arcaea_{version}.apk");

            if (!File.Exists(apkpth))
            {
                Console.WriteLine($"Downloading arcaea_{version}.apk");
                Console.WriteLine($" - from `{info.Url}`  ");
                Console.WriteLine($" - save to `{apkpth}`");
                Console.WriteLine("Note: If it is slow to download, you can also download the apk manually,");
                Console.WriteLine("close this program and place it in the specified location then restart it.");
                Console.WriteLine("Press Ctrl+C to cancel the download.");

                DownloadApk(info.Url);
            }
            else
            {
                Console.WriteLine($"arcaea_{version}.apk already exists, skip download.");
            }

            // not apk
            if (new FileInfo(apkpth).Length < 81920)
            {
                File.Delete(apkpth);
                Console.WriteLine("Download failed.");
                return;
            }

            Console.WriteLine("Download complete, start to extract the apk file.");

            try
            {
                apk = ZipFile.OpenRead(apkpth);
                ArgumentNullException.ThrowIfNull(apk);
            }
            catch
            {
                File.Delete(apkpth);
                Console.WriteLine("ZipFile.OpenRead failed.");
                throw;
            }

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

            var ms = new MemoryStream();
            using var libcocos2dcpp = apk.GetEntry("lib/arm64-v8a/libcocos2dcpp.so")!.Open();
            libcocos2dcpp.CopyTo(ms);
            AutoDecrypt(ms.ToArray(), info.Version);

            Version = info.Version;

            Console.WriteLine("Update complete.");
        }
        catch (Exception ex)
        {
            Logger.ExceptionError(ex);
            Console.WriteLine("Error occurred while updating. Please check the log file.");
        }
        finally
        {
            apk?.Dispose();
            Interlocked.Exchange(ref _running, 0);
            Console.WriteLine("Update task completed.");
        }
    }

    private static void AutoDecrypt(byte[] lib, string version)
    {
        try
        {
            var decrypt = new ArcaeaDecrypt();

            decrypt.ReadLib(lib);

            Config.ApiSalt = decrypt.GetSalt();
            Config.ApiEntry = decrypt.GetApiEntry();
            Config.Appversion = version;

            Config.CertFileName = $"cert-{version}.p12";
            File.WriteAllBytes(Path.Combine(Config.DataPath, Config.CertFileName), decrypt.GetCert());

            Config.WriteConfig();

            _configTimer = new(1800000);
            _configTimer.Elapsed += (_, _) => Reload("apiconfig.json");
            _configTimer.AutoReset = false;
            _configTimer.Start();
        }
        catch (Exception ex)
        {
            Logger.ExceptionError(ex);
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

        downloader.DownloadFileTaskAsync(url, new DirectoryInfo(Path.Combine(Config.DataPath, "update"))).Wait();
    }
}
