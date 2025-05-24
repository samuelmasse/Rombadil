namespace Rombadil.Script;

public static partial class Script
{
    internal static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    internal static readonly string RepoDir =
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(BaseDirectory))))!;
}
