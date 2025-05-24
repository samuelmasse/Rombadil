namespace Rombadil.Script;

public static partial class Script
{
    public static void Section(Action action) => action.Invoke();

    public static void Once(string key, Action action)
    {
        Dir(BaseDirectory, key, out var file);

        if (File.Exists(file))
            return;
        action.Invoke();
        File.Create(file);
    }

    public static string Link(string path)
    {
        var absolute = Path.GetFullPath(path, RepoDir);
        return new Uri(absolute).AbsoluteUri;
    }

    public static void Write(string path, string content)
    {
        var absolute = Path.GetFullPath(path, RepoDir);
        File.WriteAllText(absolute, content);
    }

    public static void Delete(string path)
    {
        var absolute = Path.GetFullPath(path, RepoDir);
        if (Directory.Exists(absolute))
            Directory.Delete(absolute, true);
    }

    public static void Copy(string src, string dst)
    {
        var absoluteSrc = Path.GetFullPath(src, RepoDir);
        var absoluteDst = Path.GetFullPath(dst, RepoDir);

        if (File.Exists(absoluteSrc))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteDst)!);
            File.Copy(absoluteSrc, absoluteDst);
        }
        else CopyDirectory(absoluteSrc, absoluteDst, true);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        var dir = new DirectoryInfo(sourceDir);
        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }

    public static List<string> List(string path)
    {
        var absolute = Path.GetFullPath(path, RepoDir);
        return Directory.EnumerateFileSystemEntries(absolute).Select(x => Path.GetRelativePath(RepoDir, x)).ToList();
    }
}
