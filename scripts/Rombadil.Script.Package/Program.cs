Info("Packaging emulator");

Info("Deleting dist directory");
Delete("dist");

var runtimes = new List<string>()
{
    "win-x64",
    "linux-x64",
    "osx-arm64"
};

var exes = runtimes.Select((runtime) =>
{
    Dir("src", "Rombadil", out var projectDir);
    Dir("dist", runtime, "Rombadil", out var outDir);

    Section(() =>
    {
        var command = string.Join(' ',
            "dotnet",
            "publish",
            projectDir,
            "-c Release",
            "--self-contained",
            "-p:PublishSingleFile=true",
            "-p:IncludeNativeLibrariesForSelfExtract=true",
            "-p:DebugType=None",
            $"-r {runtime}",
            $"-o {outDir}"
        );

        Run(command, $"Publishing for {runtime}", $"Failed to publish for {runtime}");
    });

    Dir(outDir, $"Rombadil{(runtime.StartsWith("win") ? ".exe" : "")}", out var exeFile);
    return exeFile;
}).ToList();

Success($"Emulator packaged");
exes.ForEach(x => Success($"-> {x}"));
