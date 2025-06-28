var inputArgument = new Argument<FileInfo>("input", "Input source file");
var outputOption = new Option<FileInfo>(["--output", "-o"], "Output binary file");
var configOption = new Option<FileInfo>(["--config", "-c"], "Config file");

var rootCommand = new RootCommand("Rombadil Assembler (rombadilasm)")
{
    inputArgument,
    outputOption,
    configOption
};

int exitCode = 1;

rootCommand.SetHandler((input, output, config) =>
{
    if (!input.Exists)
    {
        PrintError($"{input.Name}: ", "File not found");
        return;
    }

    var segments = DefaultSegments();

    if (config != null)
    {
        if (!config.Exists)
        {
            PrintError($"{config.Name}: ", "Config not found");
            return;
        }

        try
        {
            segments = ParseSegments(File.ReadAllText(config.FullName));
        }
        catch
        {
            PrintError($"{config.Name}: ", "Failed to parse segments");
            return;
        }
    }

    string[] source = File.ReadAllLines(input.FullName);
    byte[]? binary;

    try
    {
        binary = new Assembler6502(segments).Assemble(source);
    }
    catch (Assembler6502Exception e)
    {
        PrintError($"{input.Name}:{e.Line + 1}: ", e.Error.TrimEnd('.'));
        return;
    }

    string dir = Path.GetDirectoryName(input.FullName)!;
    string file = $"{Path.GetFileNameWithoutExtension(input.FullName)}.bin";

    if (output?.DirectoryName != null)
    {
        dir = output.DirectoryName;
        file = output.FullName;
    }

    Directory.CreateDirectory(dir);
    File.WriteAllBytes(file, binary);
    exitCode = 0;

}, inputArgument, outputOption, configOption);

await rootCommand.InvokeAsync(args);
return exitCode;

static void PrintError(string context, string message)
{
    Console.Error.Write(context);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Error.Write("error: ");
    Console.ResetColor();
    Console.Error.WriteLine(message);
}

static List<AssemblerSegment> ParseSegments(string tomlContent)
{
    var model = Toml.Parse(tomlContent).ToModel();
    var segments = new List<AssemblerSegment>();

    foreach (var kvp in model)
    {
        if (kvp.Value is not TomlTable table)
            continue;

        var name = kvp.Key;
        var memoryStart = Convert.ToInt32(table["MemoryStart"]);
        var fileSize = Convert.ToInt32(table["FileSize"]);

        segments.Add(new AssemblerSegment(name, memoryStart, fileSize));
    }

    return segments;
}

static List<AssemblerSegment> DefaultSegments()
{
    return
    [
        new("Header", 0x0000, 0x0010),
        new("Code", 0x8000, 0x7FFA),
        new("Vectors", 0xFFFA, 0x0006),
        new("Chars", 0x0000, 0x2000)
    ];
}
