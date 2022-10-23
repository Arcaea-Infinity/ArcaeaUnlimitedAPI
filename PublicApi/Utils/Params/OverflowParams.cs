namespace ArcaeaUnlimitedAPI.PublicApi.Params;

internal sealed record OverflowParams(string Overflow) : IParams<int>
{
    public int Validate(out Response? error)
    {
        error = null;
        var overflowCount = 0;
        if (!string.IsNullOrWhiteSpace(Overflow) && (!int.TryParse(Overflow, out overflowCount) || overflowCount is < 0 or > 10))
            error = Response.Error.InvalidRecentOrOverflowNumber;
        return overflowCount;
    }
}
