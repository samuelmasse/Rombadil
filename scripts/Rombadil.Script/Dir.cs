namespace Rombadil.Script;

public static partial class Script
{
    public static void Dir(string part1, string part2, out string path) =>
        Dir([part1, part2], out path);

    public static void Dir(string part1, string part2, string part3, out string path) =>
        Dir([part1, part2, part3], out path);

    public static void Dir(string part1, string part2, string part3, string part4, out string path) =>
        Dir([part1, part2, part3, part4], out path);

    public static void Dir(string[] parts, out string path) =>
        path = Path.Combine(parts);
}
