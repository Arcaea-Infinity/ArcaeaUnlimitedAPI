using System.Collections.Concurrent;

namespace ArcaeaUnlimitedAPI.PublicApi.Params;

public record DifficultyParams(string? Difficulty) : IParams<sbyte>
{
    private static readonly ConcurrentDictionary<sbyte, string[]> List;

    static DifficultyParams()
    {
        List = new();
        List.TryAdd(0, new[] { "0", "pst", "past" });
        List.TryAdd(1, new[] { "1", "prs", "present" });
        List.TryAdd(2, new[] { "2", "ftr", "future" });
        List.TryAdd(3, new[] { "3", "byn", "byd", "beyond" });
    }

    public sbyte Validate(out Response? error)
    {
        error = null;

        if (Difficulty is null)
        {
            error = Response.Error.InvalidDifficulty;
            return 2;
        }

        foreach (var (index, alias) in List)
            if (alias.Any(t => string.Equals(t, Difficulty, StringComparison.OrdinalIgnoreCase)))
                return index;

        error = Response.Error.InvalidDifficulty;
        return -1;
    }
}
