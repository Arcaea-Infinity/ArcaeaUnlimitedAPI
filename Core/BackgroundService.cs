﻿using System.Diagnostics;
using System.IO.Compression;
using System.Timers;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Json.Songlist;
using Newtonsoft.Json;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;
using static ArcaeaUnlimitedAPI.Core.Utils;
using Timer = System.Timers.Timer;

namespace ArcaeaUnlimitedAPI.Core;

internal static class BackgroundService
{
    internal static ulong TimerCount;

    private static string? _version;

    private static volatile object _lockobj = new();

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
        if (TimerCount % 12 == 0) Config.Nodes.AsParallel().Select(TestNode);
    }

    private static string Version
    {
        get => _version ??= File.ReadAllText($"{Config.DataRootPath}/arcversion");
        set
        {
            _version = value;
            File.WriteAllText($"{Config.DataRootPath}/arcversion", value);
        }
    }

    internal static void ArcUpdate(object? source, ElapsedEventArgs? e)
    {
        lock (_lockobj)
        {
            var info = GetLatestVersion().Result;
            if (info?.Url is null || Version == info.Version) return;
            if (Config.Appversion != info.Version) NeedUpdate = true;
            var dirname = info.Version;
            var apkpth = $"{Config.DataRootPath}/update/arcaea_{dirname}.apk";
            var dirpth = $"{Config.DataRootPath}/update/{dirname}dir/";

            try
            {
                if (!File.Exists(apkpth)) DownloadApk(info.Url);

                if (Directory.Exists(dirpth)) Directory.Delete(dirpth, true);
                Directory.CreateDirectory(dirpth);
                ZipFile.ExtractToDirectory(apkpth, dirpth);

                foreach (var file in new DirectoryInfo($"{dirpth}/assets/char/").GetFiles()
                                                                                .Where(file =>
                                                                                           !File
                                                                                               .Exists($"{Config.DataRootPath}/sourse/char/{file.Name}")))
                    file.MoveTo($"{Config.DataRootPath}/sourse/char/{file.Name}");

                var list = JsonConvert.DeserializeObject<Songlist>(File.ReadAllText($"{dirpth}/assets/songs/songlist"));

                if (list is not null)
                    foreach (var i in list.Songs)
                    {
                        var destdir = $"{Config.DataRootPath}/sourse/songs";
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

                        Thread.Sleep(300);
                        ArcaeaSongs.Insert(i);
                    }

                Version = info.Version;
                File.Delete(apkpth);
            }
            catch (Exception ex)
            {
                Log.ExceptionError(ex);
            }
            finally
            {
                Directory.Delete(dirpth, true);
            }
        }
    }

    private static void DownloadApk(string url)
    {
        var psi = new ProcessStartInfo { FileName = "aria2c", Arguments = $"--dir=/etc/botarcapi/data/update/ {url}" };
        using var p = Process.Start(psi);
        p?.WaitForExit();
        p?.Close();
    }
}
