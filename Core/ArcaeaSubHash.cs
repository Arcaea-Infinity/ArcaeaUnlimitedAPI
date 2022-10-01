using Newtonsoft.Json;

#pragma warning disable CS8618

namespace ArcaeaUnlimitedAPI.Core;

public class ArcaeaSubHash
{
    private static readonly HttpClient Client = new();

    private string GetString(string method, string body, string path)
        => Client
          .GetStringAsync($"{GlobalConfig.Config.ChallengeUrl}?method={method}&path={Uri.EscapeDataString(path)}&body={Uri.EscapeDataString(body)}")
          .Result;

    internal Func<string, string, string, ulong, string> GenerateChallenge;

    internal void Init()
    {
        if (GlobalConfig.Config.ChallengeType != "taikari")
        {
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GlobalConfig.Config.ChallengeToken}");
            GenerateChallenge = (
                                    method,
                                    body,
                                    path,
                                    _) => JsonConvert.DeserializeObject<ChallengeApiRoot>(GetString(method, body, path))?.Content!;
        }
        else
        {
            GenerateChallenge = (
                                    method,
                                    body,
                                    path,
                                    _) => JsonConvert.DeserializeObject<TaikariRoot>(GetString(method, body, path))?.Content?.Challenge!;
        }
    }

    public class TaikariContent
    {
        [JsonProperty("challenge")]
        public string Challenge { get; set; }
    }

    public class TaikariRoot
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("content")]
        public TaikariContent? Content { get; set; }
    }

    public class ChallengeApiRoot
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
