namespace ArcaeaUnlimitedAPI.Beans;

#pragma warning disable CS8618

internal class DifficultyInfo
{
    private static readonly List<DifficultyInfo> List = new()
                                                        {
                                                            new() { Index = 0, Alias = new[] { "0", "pst", "past" } },
                                                            new()
                                                            {
                                                                Index = 1, Alias = new[] { "1", "prs", "present" }
                                                            },
                                                            new() { Index = 2, Alias = new[] { "2", "ftr", "future" } },
                                                            new()
                                                            {
                                                                Index = 3, Alias = new[] { "3", "byn", "byd", "beyond" }
                                                            }
                                                        };

    private sbyte Index { get; init; }
    private string[] Alias { get; init; }

    internal static bool TryParse(string? dif, out sbyte value)
    {
        value = dif is null
            ? (sbyte)-1
            : List
              .FirstOrDefault(info => info.Alias.Any(i => string.Equals(i, dif, StringComparison.OrdinalIgnoreCase)))
              ?.Index ?? -1;
        return value != -1;
    }
}
