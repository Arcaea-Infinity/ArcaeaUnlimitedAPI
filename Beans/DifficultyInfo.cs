using System.Collections.Concurrent;

namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

internal static class DifficultyInfo
{
    private static readonly ConcurrentDictionary<sbyte, string[]> List;

    static DifficultyInfo()
    {
        List = new();

        List.TryAdd(0, new[] { "0", "pst", "past" });
        List.TryAdd(1, new[] { "1", "prs", "present" });
        List.TryAdd(2, new[] { "2", "ftr", "future" });
        List.TryAdd(3, new[] { "3", "byn", "byd", "beyond" });
    }

    internal static bool TryParse(string? dif, out sbyte value)
    {
        if (dif is null)
        {
            value = -1;
            return false;
        }

        foreach (var (index,alias) in List)
        {
            if (alias.Any(t => string.Equals(t, dif, StringComparison.OrdinalIgnoreCase)))
            {
                value = index;
                return true;
            }
        }

        {
            value = -1;
            return false;
        }
    }
}
