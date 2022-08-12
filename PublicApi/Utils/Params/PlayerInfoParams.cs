using ArcaeaUnlimitedAPI.Beans;

namespace ArcaeaUnlimitedAPI.PublicApi.Params;

public record PlayerInfoParams(string? User, string? UserCode) : IParams<PlayerInfo>
{
    public PlayerInfo? Validate(out Response? error)
    {
        error = null;

        if (!string.IsNullOrWhiteSpace(UserCode))
        {
            if (!int.TryParse(UserCode, out var ucode) || ucode is < 0 or > 999999999)
            {
                error = Response.Error.InvalidUsercode;
                return null;
            }

            // use this user code directly
            return PlayerInfo.GetByCode(UserCode).FirstOrDefault()
                   ?? new PlayerInfo { Code = UserCode.PadLeft(9, '0') };
        }

        if (string.IsNullOrWhiteSpace(User))
        {
            error = Response.Error.InvalidUserNameorCode;
            return null;
        }

        var players = PlayerInfo.GetByAny(User);

        if (players.Count == 0)
        {
            if (!int.TryParse(User, out var ucode) || ucode is < 0 or > 999999999)
            {
                error = Response.Error.UserNotFound;
                return null;
            }

            return new() { Code = User.PadLeft(9, '0') };
        }

        if (players.Count > 1)
        {
            error = Response.Error.TooManyUsers;
            return null;
        }

        return players[0];
    }
}
