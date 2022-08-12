namespace ArcaeaUnlimitedAPI.PublicApi.Params;

public record OverflowParams(string? Overflow) : IParams<int>
{
    public int Validate(out Response? error)
    {
        error = null;
        var overflowCount = 0;
        if (Overflow is not null && (!int.TryParse(Overflow, out overflowCount) || overflowCount is < 0 or > 10))
            error = Response.Error.InvalidRecentOrOverflowNumber;
        return overflowCount;
    }
}
