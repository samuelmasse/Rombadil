var repoRoot = FindRepositoryRoot();
var project = Path.Combine(repoRoot, "src", "Rombadil", "Rombadil.csproj");
var dist = Path.Combine(repoRoot, "dist");
var runtimes = new[] { "win-x64", "linux-x64", "osx-arm64" };

Console.WriteLine("Packaging emulator");
ResetDirectory(dist);

var executables = new List<string>();
foreach (var runtime in runtimes)
{
    var output = Path.Combine(dist, runtime, "Rombadil");
    await PublishAsync(repoRoot, project, output, runtime);
    executables.Add(Path.Combine(output, "Rombadil" + (runtime.StartsWith("win", StringComparison.Ordinal) ? ".exe" : "")));
}

Console.WriteLine("Emulator packaged");
foreach (var executable in executables)
    Console.WriteLine("-> " + executable);

static string FindRepositoryRoot()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var current = Path.GetFullPath(start); current is not null; current = Directory.GetParent(current)?.FullName)
        {
            if (File.Exists(Path.Combine(current, "Rombadil.slnx")))
                return current;
        }
    }

    throw new InvalidOperationException("Rombadil.slnx not found above the current process directories.");
}

static void ResetDirectory(string path)
{
    Console.WriteLine("Deleting " + path);
    if (Directory.Exists(path))
        Directory.Delete(path, recursive: true);
    Directory.CreateDirectory(path);
}

static async Task PublishAsync(string repoRoot, string project, string output, string runtime)
{
    Console.WriteLine("Publishing for " + runtime);
    Directory.CreateDirectory(output);

    using var process = new Process
    {
        StartInfo =
        {
            FileName = "dotnet",
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };

    foreach (var argument in new[]
    {
        "publish",
        project,
        "-c",
        "Release",
        "--self-contained",
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:DebugType=None",
        "-r",
        runtime,
        "-o",
        output
    })
    {
        process.StartInfo.ArgumentList.Add(argument);
    }

    process.Start();
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    var text = await outputTask + await errorTask;
    Console.Write(text);

    if (process.ExitCode != 0)
        throw new InvalidOperationException($"Publishing for {runtime} failed with exit code {process.ExitCode}.");
}
