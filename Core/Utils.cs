using System.Text.RegularExpressions;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using Newtonsoft.Json;

namespace ArcaeaUnlimitedAPI.Core;

internal static class Utils
{
    internal static double CalcSongRating(int score, double @const)
        => score switch
           {
               >= 10000000 => @const / 10 + 2,
               >= 9800000  => @const / 10 + 1 + (double)(score - 9800000) / 200000,
               _           => Math.Max(0, @const / 10 + (double)(score - 9500000) / 300000)
           };

    internal static async Task<ArcUpdateValue?> GetLatestVersion()
    {
        try
        {
            var obj
                = JsonConvert.DeserializeObject<ResponseRoot>(await WebHelper
                                                                 .GetString("https://webapi.lowiro.com/webapi/serve/static/bin/arcaea/apk"));

            return obj?.Success != true ? null : obj.DeserializeContent<ArcUpdateValue>();
        }
        catch (Exception ex)
        {
            Logger.ExceptionError(ex);
            return null;
        }
    }

    private static class WebHelper
    {
        private static readonly HttpClient Client;

        static WebHelper()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (
                _,
                _,
                _,
                _) => true;
            Client = new(handler);
        }

        internal static async Task<string> GetString(string url) => await Client.GetStringAsync(new Uri(url));
    }

    internal static class StringCompareHelper
    {
        private static readonly Regex Reg = new(@"\s|\(|\)|（|）", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static bool Contains(string? raw, string? seed)
            => seed != null &&
               raw != null &&
               Reg.Replace(raw, string.Empty).Contains(Reg.Replace(seed, string.Empty), StringComparison.OrdinalIgnoreCase);

        internal static bool Equals(string? raw, string? seed)
            => seed != null &&
               raw != null &&
               string.Equals(Reg.Replace(raw, string.Empty), Reg.Replace(seed, string.Empty), StringComparison.OrdinalIgnoreCase);
    }

    internal static class RandomHelper
    {
        private static readonly Random Random = new();

        internal static T? GetRandomItem<T>(T?[] ls) => ls.Any() ? ls[Random.Next(ls.Length)] : default;
    }
}
