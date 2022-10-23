namespace ArcaeaUnlimitedAPI.PublicApi.Params;

internal sealed record RecentParams(string Recent) : IParams<int>
{
    public int Validate(out Response? error)
    {
        error = null;
        var recentCount = 1;
        if (!string.IsNullOrWhiteSpace(Recent))
            if (!int.TryParse(Recent, out recentCount) || recentCount is < 0 or > 7)
                error = Response.Error.InvalidRecentOrOverflowNumber;
        return recentCount;
    }
}
