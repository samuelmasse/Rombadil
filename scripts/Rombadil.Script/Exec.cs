namespace Rombadil.Script;

public static partial class Script
{
    public static bool Exec(string command, string args)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = RepoDir
            }
        };

        process.Start();
        process.WaitForExit();

        return process.ExitCode == 0;
    }

    public static void Run(string fullCommand, string description, string errorMessage)
    {
        Info(description);

        var parts = fullCommand.Split(' ');
        var command = parts[0];
        var args = string.Join(' ', parts.Skip(1));

        var stopwatch = Stopwatch.StartNew();
        bool res = Exec(command, args);
        stopwatch.Stop();

        if (res)
            Success($"{description} ({stopwatch.Elapsed.TotalSeconds:F2}s)");
        else
        {
            Fail($"{errorMessage}");
            Environment.Exit(1);
        }
    }

    public static void Read(string fullCommand, string description, string errorMessage, out string output)
    {
        Info(description);

        var parts = fullCommand.Split(' ');
        var command = parts[0];
        var args = string.Join(' ', parts.Skip(1));

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = RepoDir
            }
        };

        process.Start();
        output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
    }
}
