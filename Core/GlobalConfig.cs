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
            File.WriteAllText(apiconfig, JsonConvert.SerializeObject(new ConfigItem(), Formatting.Indented));
            Console.WriteLine("apiconfig.json not found, created a new one.");
            Console.WriteLine("Please fill in the information and restart the program.");
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
            Console.WriteLine("Certificate file not found, please put it in the data folder and restart the program.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }

    internal static void Init(string fileName)
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
}
