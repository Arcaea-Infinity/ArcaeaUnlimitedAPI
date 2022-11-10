using System.Collections.Concurrent;
using ArcaeaUnlimitedAPI.Core;
using SQLite;

// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable once UnusedAutoPropertyAccessor.Global

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

[Table("accounts")]
[DatabaseManager.CreateTableSqlAttribute("CREATE TABLE IF NOT EXISTS `accounts` (`name` TEXT NOT NULL,`passwd` TEXT NOT NULL,`device` TEXT NOT NULL DEFAULT '',`uid` INTEGER DEFAULT 0,`ucode`  TEXT DEFAULT '', `token`  TEXT DEFAULT '',`banned` TEXT NOT NULL DEFAULT 'false' CHECK(`banned` IN('true', 'false')),PRIMARY KEY (`name` ASC));")]
internal sealed class AccountInfo
{
    private static readonly Lazy<ConcurrentQueue<AccountInfo>> Queue
        = new(new ConcurrentQueue<AccountInfo>(DatabaseManager.Account.Where<AccountInfo>(i => i.Banned != "true")));

    internal static bool IsEmpty => Queue.Value.IsEmpty;

    [PrimaryKey] [Column("name")]
    public string Name { get; set; }

    [Column("passwd")]
    public string Password { get; set; }

    [Column("device")]
    public string DeviceID { get; set; }

    [Column("uid")]
    public int UserID { get; set; }

    [Column("ucode")]
    public string Code { get; set; }

    [Column("token")]
    public string Token { get; set; }

    [Column("banned")]
    public string Banned { get; set; }

    [Ignore] public string CurrentTokenId { get; private set; }

    internal static async Task<AccountInfo?> Alloc(string tokenid)
    {
        AccountInfo? account = null;
        while (true)
        {
            try
            {
                if (GlobalConfig.IllegalHash || GlobalConfig.NeedUpdate) return null;

                if (!Queue.Value.TryDequeue(out account))
                {
                    Logger.FunctionError("Account", "ranout.");
                    return null;
                }

                account.CurrentTokenId = tokenid;

                if (account.Banned == "true") continue;

                if (string.IsNullOrEmpty(account.Token) && !await account.GetToken())
                {
                    if (account.Banned != "true") Recycle(account);
                    continue;
                }

                if (!await account.ClearFriend())
                {
                    Recycle(account);
                    continue;
                }

                return account;
            }
            catch (Exception ex)
            {
                Logger.ExceptionError(ex);
                Recycle(account);
                continue;
            }
        }
    }

    internal static void Recycle(AccountInfo? info)
    {
        if (info is not null) Queue.Value.Enqueue(info);
    }

    internal static void Insert(AccountInfo info)
    {
        DatabaseManager.Account.Insert(info);
        Queue.Value.Enqueue(info);
    }
}
