var inputArgument = new Argument<FileInfo>("input") { Description = "Input source file" };
var outputOption = new Option<FileInfo?>("--output", "-o") { Description = "Output binary file" };
var configOption = new Option<FileInfo?>("--config", "-c") { Description = "Config file" };

var rootCommand = new RootCommand("Rombadil Assembler (rombadilasm)")
{
    inputArgument,
    outputOption,
    configOption
};

rootCommand.SetAction(parseResult =>
{
    var input = parseResult.GetValue(inputArgument)!;
    var output = parseResult.GetValue(outputOption);
    var config = parseResult.GetValue(configOption);

    if (!input.Exists)
    {
        PrintError($"{input.Name}: ", "File not found");
        return 1;
    }

    var segments = DefaultSegments();

    if (config != null)
    {
        if (!config.Exists)
        {
            PrintError($"{config.Name}: ", "Config not found");
            return 1;
        }

        try
        {
            segments = ParseSegments(File.ReadAllText(config.FullName));
        }
        catch
        {
            PrintError($"{config.Name}: ", "Failed to parse segments");
            return 1;
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
        return 1;
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
    return 0;
});

return await rootCommand.Parse(args).InvokeAsync();

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
    var model = TomlSerializer.Deserialize<Dictionary<string, SegmentEntry>>(tomlContent)
                ?? throw new InvalidDataException("Empty TOML");
    var segments = new List<AssemblerSegment>();

    foreach (var (name, entry) in model)
        segments.Add(new AssemblerSegment(name, entry.MemoryStart, entry.FileSize));

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

class SegmentEntry
{
    public int MemoryStart { get; set; }
    public int FileSize { get; set; }
}
