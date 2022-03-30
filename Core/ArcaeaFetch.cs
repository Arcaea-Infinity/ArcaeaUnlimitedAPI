using System.Security.Cryptography.X509Certificates;
using System.Text;
using ArcaeaUnlimitedAPI.Beans;
using ArcaeaUnlimitedAPI.Json.ArcaeaFetch;
using ArcaeaUnlimitedAPI.PublicApi;
using Newtonsoft.Json;
using static ArcaeaUnlimitedAPI.Core.GlobalConfig;

namespace ArcaeaUnlimitedAPI.Core;

internal static class ArcaeaFetch
{
    internal static string GenerateChallenge(string method, string body, string path, ulong time = 0) =>
        _arcaeaHash.GenerateChallenge(method, body, path, time);

    internal static async Task<bool> GetToken(this AccountInfo accountInfo)
    {
        try
        {
            var info = await Login(accountInfo);

            if (info?.Success == true)
            {
                accountInfo.Token = info.AccessToken!;
                DatabaseManager.Account.Update(accountInfo);
                return true;
            }

            if (info?.ErrorCode == 5) NeedUpdate = true;

            if (info?.ErrorCode == 106)
            {
                accountInfo.Banned = "true";
                DatabaseManager.Account.Update(accountInfo);
            }

            return false;
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
            return false;
        }
    }

    internal static async Task<(bool, List<FriendsItem>?)> UserMe(this AccountInfo accountInfo, bool tryagain = true)
    {
        var info = await Get("user/me", accountInfo, null);
        if (info is null) return (false, null);

        if (info.Success)
        {
            var value = info.DeserializeContent<UserMeValue>();
            accountInfo.UserID = value.UserID;
            accountInfo.Ucode = value.UserCode;
            DatabaseManager.Account.Update(accountInfo);
            return (true, value.Friends);
        }

        if (info.ErrorCode == 5)
        {
            NeedUpdate = true;
            return (false, null);
        }

        if (info.Code == "UnauthorizedError" || info.ErrorCode != null)
        {
            accountInfo.Token = "";
            DatabaseManager.Account.Update(accountInfo);
            if (!tryagain || !await accountInfo.GetToken()) return (false, null);
            return await accountInfo.UserMe(false);
        }

        return (false, null);
    }

    internal static async Task<bool> ClearFriend(this AccountInfo accountInfo)
    {
        var (success, friends) = await accountInfo.UserMe();

        if (success)
            foreach (var friend in friends!)
                await accountInfo.DeleteFriend(friend.UserID.ToString());

        return success;
    }

    private static async Task DeleteFriend(this AccountInfo accountInfo, string friendID) =>
        await Post("friend/me/delete", accountInfo, new() { { "friend_id", friendID } });

    internal static async Task<(bool, List<FriendsItem>?)> AddFriend(this AccountInfo accountInfo, string usercode)
    {
        var info = await Post("friend/me/add", accountInfo, new() { { "friend_code", usercode } });

        if (info is null) return (false, null);

        if (info.ErrorCode == 5)
        {
            NeedUpdate = true;
            return (false, null);
        }

        if (info.Value is null) return (false, null);
        var value = info.DeserializeContent<AddFriendValue>();
        return (info.Success, value.Friends);
    }

    internal static async Task<(bool, List<Records>?)> FriendRank(this AccountInfo accountInfo, string songID,
                                                                  int difficulty)
    {
        try
        {
            var info = await Get("score/song/friend", accountInfo,
                                 new()
                                 {
                                     { "song_id", songID },
                                     { "difficulty", difficulty.ToString() },
                                     { "start", "0" },
                                     { "limit", "11" }
                                 });

            if (info is null) return (false, null);

            if (info.ErrorCode == 5)
            {
                NeedUpdate = true;
                return (false, null);
            }

            if (info.Value is null) return (false, null);
            var value = info.DeserializeContent<List<Records>>();
            return (info.Success, value);
        }
        catch (Exception ex)
        {
            Log.ExceptionError(ex);
            return (false, null);
        }
    }

    internal static async Task RegisterTask()
    {
        if (BackgroundService.TimerCount % 144 != 0) return;

        foreach (var node in Config.Nodes)
            try
            {
                for (var i = 0; i < 3; ++i)
                {
                    var name = RandomStringGenerator.GetRandString();
                    var password = RandomStringGenerator.GetRandString();
                    var email = RandomStringGenerator.GetRandString() + "@gmail.com";
                    var deviceID = RandomStringGenerator.GetRandDeviceID();

                    await Task.Delay(300);

                    var info = await Register(node, name, password, email, deviceID);

                    if (info?.ErrorCode == 124) return;
                    if (info?.ErrorCode == 5)
                    {
                        NeedUpdate = true;
                        return;
                    }

                    if (info?.Success == true)
                    {
                        var value = info.DeserializeContent<RegisterValue>();
                        var account = new AccountInfo
                                      {
                                          Name = name,
                                          Password = password,
                                          DeviceId = deviceID,
                                          UserID = value.UserID,
                                          Token = value.AccessToken,
                                          Banned = "false"
                                      };

                        AccountInfo.Insert(account);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ExceptionError(ex);
                continue;
            }
    }

    private static class RandomStringGenerator
    {
        private const string TemplateHex = "0123456789abcdef";

        private const string TemplateNormal = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkimnopqrstuvwxyz";

        private static readonly Random Random = new();

        public static string GetRandString()
        {
            var res = new StringBuilder();
            for (var i = 0; i < 10; i++) res.Append(TemplateNormal[Random.Next(36)]);
            return res.ToString();
        }

        public static string GetRandDeviceID()
        {
            var res = new StringBuilder();
            for (var i = 0; i < 32; i++) res.Append(TemplateHex[Random.Next(16)]);
            return res.ToString();
        }
    }

#region Privite Methods

    private static HttpClient _client = null!;
    private static string _apientry = null!;
    private static readonly int _maxRetryCount = 3;
    private static ArcaeaHash _arcaeaHash = null!;

    internal static void Init()
    {
        _apientry = Config.ApiEntry;
        _arcaeaHash = new();
        _arcaeaHash.Init();
        var certificate = new X509Certificate2($"{Config.DataPath}/{Config.CertFileName}", Config.CertPassword);
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(certificate);
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        _client = new(handler);
        _client.Timeout = TimeSpan.FromSeconds(30);
        _client.DefaultRequestHeaders.Add("Host", Config.Host);
        _client.DefaultRequestHeaders.Add("AppVersion", Config.Appversion);
        _client.DefaultRequestHeaders.Add("Platform", "android");
        _client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
        _client.DefaultRequestHeaders.Add("User-Agent",
                                          "Dalvik/2.1.0 (Linux; U; Android 9; G8142 Build/47.2.A.10.107)");
    }

    private static async Task<ResponseRoot?> Get(string resturl, AccountInfo info,
                                                 Dictionary<string, string>? submitData, byte retryCount = 0)
    {
        if (NeedUpdate) return default;

        var url = submitData is not null
            ? $"{resturl}?{SubmitDataToString(submitData)}"
            : resturl;

        var node = NodeInfo.Alloc();
        if (node is null) return default;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{node}/{_apientry}/{url}");
        request.Headers.Add("DeviceId", info.DeviceId);
        request.Headers.Add("Accept-Encoding", "identity");
        request.Headers.Authorization = new("Bearer", info.Token);
        request.Headers.Add("X-Random-Challenge", GenerateChallenge("GET", "", url));
        request.Headers.Add("i", info.UserID.ToString());

        var (success, result) = await LogResult(resturl, request);
        if (success) return result;

        if (retryCount < _maxRetryCount) return await Get(resturl, info, submitData, ++retryCount);
        Log.ApiError(resturl);
        return default;
    }

    private static async Task<ResponseRoot?> Post(string resturl, AccountInfo info,
                                                  Dictionary<string, string>? submitData, byte retryCount = 0)
    {
        if (NeedUpdate) return default;
        var data = submitData is null
            ? ""
            : SubmitDataToString(submitData);


        var node = NodeInfo.Alloc();
        if (node is null) return default;

        var request = new HttpRequestMessage(HttpMethod.Post, $"{node}/{_apientry}/{resturl}");
        request.Headers.Add("DeviceId", info.DeviceId);
        request.Headers.Add("Accept-Encoding", "identity");
        request.Headers.Authorization = new("Bearer", info.Token);
        request.Content = new StringContent(data, Encoding.UTF8, new("application/x-www-form-urlencoded"));
        request.Headers.Add("X-Random-Challenge", GenerateChallenge("POST", data, resturl));
        request.Headers.Add("i", info.UserID.ToString());

        var (success, result) = await LogResult(resturl, request);
        if (success) return result;


        if (retryCount < _maxRetryCount) return await Post(resturl, info, submitData, ++retryCount);
        Log.ApiError(resturl);
        return default;
    }

    private static async Task<ResponseRoot?> Login(AccountInfo info, byte retryCount = 0)
    {
        if (NeedUpdate) return default;

        var resturl = "auth/login";
        var data = "grant_type=client_credentials";

        var node = NodeInfo.Alloc();
        if (node is null) return default;

        var request = new HttpRequestMessage(HttpMethod.Post, $"{node}/{_apientry}/{resturl}");
        request.Headers.Add("Accept-Encoding", "identity");
        request.Headers.Authorization
            = new("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{info.Name}:{info.Password}")));
        request.Headers.Add("DeviceId", info.DeviceId);
        request.Content = new StringContent(data, Encoding.UTF8, new("application/x-www-form-urlencoded"));
        request.Headers.Add("X-Random-Challenge", GenerateChallenge("POST", data, resturl));
        request.Headers.Add("i", info.UserID.ToString());

        var (success, result) = await LogResult(resturl, request);
        if (success) return result;

        if (retryCount < _maxRetryCount) return await Login(info, ++retryCount);
        Log.ApiError(resturl);
        return default;
    }

    private static async Task<ResponseRoot?> Register(Node node, string name, string password, string email,
                                                      string deviceID)
    {
        if (NeedUpdate) return default;
        var resturl = "user";
        var data = SubmitDataToString(new()
                                      {
                                          { "name", name },
                                          { "password", password },
                                          { "email", email },
                                          { "device_id", deviceID }
                                      });
        var request = new HttpRequestMessage(HttpMethod.Post, $"{node}/{_apientry}/{resturl}");
        request.Content = new StringContent(data, Encoding.UTF8, new("application/x-www-form-urlencoded"));
        request.Headers.Add("X-Random-Challenge", GenerateChallenge("POST", data, resturl));

        var (_, result) = await LogResult(resturl, request);
        return result;
    }


    private static async Task<(bool, ResponseRoot?)> LogResult(string resturl, HttpRequestMessage request)
    {
        try
        {
            var resp = await _client.SendAsync(request);
            var success = resp.Content.Headers.ContentType?.ToString() == "application/json";
            if (!success)
            {
                Log.ApiError(resturl, (int)resp.StatusCode);
                return (false, default);
            }

            var result = JsonConvert.DeserializeObject<ResponseRoot>(await resp.Content.ReadAsStringAsync())!;
            if (!result.Success && result.ErrorCode != 401) Log.ApiError(resturl, result);

            return (success, result);
        }
        catch (Exception ex)
        {
            Log.HttpError(ex.Message, request.RequestUri);
            return (false, default);
        }
        finally
        {
            request.Dispose();
        }
    }

    private static string SubmitDataToString(Dictionary<string, string> submitData) =>
        new FormUrlEncodedContent(submitData).ReadAsStringAsync().Result;

#endregion
}
