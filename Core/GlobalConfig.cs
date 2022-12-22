using ArcaeaUnlimitedAPI.Beans;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class GlobalConfig
{
    internal static ConfigItem Config = null!;
    internal static HashSet<string> Tokens = null!;

    internal static volatile bool NeedUpdate = false;
    internal static volatile bool IllegalHash = false;

    internal static void Init()
    {
        var apiconfig = Path.Combine(AppContext.BaseDirectory, "apiconfig.json");

        if (!File.Exists(apiconfig))
        {
            Console.WriteLine("apiconfig.json not found, get infomation from main server...");

            Dictionary<string, string> json = CertResponse.GetContent();

            Config = new()
                     {
                         ApiEntry = json["entry"],
                         Appversion = json["version"],
                         Host = "arcapi-v2.lowiro.com",
                         CertFileName = $"cert-{json["version"]}.p12",
                         CertPassword = json["password"],
                         DataPath = Path.Combine(AppContext.BaseDirectory, "data"),
                         ApiSalt = Array.Empty<byte>(),
                         Node = new() { Url = "arcapi-v2.lowiro.com" }
                     };

            Console.WriteLine($"set the data_path as \"{Config.DataPath}\"");

            File.WriteAllText(apiconfig, JsonConvert.SerializeObject(Config, Formatting.Indented));
            Console.WriteLine("apiconfig.json created.");

            Directory.CreateDirectory(Config.DataPath);
            File.WriteAllBytes(Path.Combine(Config.DataPath, Config.CertFileName), Convert.FromBase64String(json["cert"]));
            Console.WriteLine("cert file created.");

            Console.WriteLine("Please fill in the information into apiconfig.json and restart the program.");
            Console.WriteLine("Folders needed by the program will be created automatically after restart.");
            Console.WriteLine("Other information can be found in the README.md and deploy.md.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;

        var tokens = Path.Combine(AppContext.BaseDirectory, "tokens.json");
        if (!File.Exists(tokens)) File.WriteAllText(tokens, "[]");
        Tokens = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText("tokens.json"))!;

        Directory.CreateDirectory(Path.Combine(Config.DataPath, "log"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "database"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "update"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "source", "songs"));
        Directory.CreateDirectory(Path.Combine(Config.DataPath, "source", "char"));

        var arcversion = Path.Combine(Config.DataPath, "arcversion");
        if (!File.Exists(arcversion)) File.WriteAllText(arcversion, "4.0.0c");

        var arccert = Path.Combine(Config.DataPath, Config.CertFileName);

        if (!File.Exists(arccert))
        {
            Console.WriteLine("cert file not found, get infomation from main server...");

            Dictionary<string, string> json = CertResponse.GetContent();
            File.WriteAllBytes(Config.CertFileName, Convert.FromBase64String(json["cert"]));

            Console.WriteLine("cert file created.");
        }
    }

    internal static void CheckUpdate()
    {
        if (AccountInfo.IsEmpty)
        {
            Console.WriteLine("Account pool is empty. Do you need to register some accounts automatically? (y/n)");
            Console.WriteLine("[Note: You can configure enable or disable `Auto Register Account Task` to daily scheduled tasks in apiconfig.json.]");

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.WriteLine();
                Console.WriteLine("Register account task running...");
                ArcaeaFetch.RegisterTask();
                Console.WriteLine("Register account task completed.");
            }
        }
        
        BackgroundService.ArcUpdate();

        Console.Clear();
    }

    internal static void Reload(string fileName)
    {
        switch (fileName)
        {
            case "apiconfig.json":
                Config = JsonConvert.DeserializeObject<ConfigItem>(File.ReadAllText("apiconfig.json"))!;
                ArcaeaFetch.Init();
                break;

            case "tokens.json":
                Tokens = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText("tokens.json"))!;
                break;
        }
    }

#pragma warning disable CS8618

    [Serializable]
    internal class CertResponse
    {
        [JsonProperty("content")]
        public Dictionary<string, string> Content { get; set; }

        internal static Dictionary<string, string> GetContent()
            => JsonConvert.DeserializeObject<CertResponse>(Utils.WebHelper.GetString("https://server.awbugl.top/botarcapi/data/cert").Result)!
                          .Content;
    }
}
