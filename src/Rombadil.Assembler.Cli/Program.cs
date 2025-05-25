using System.CommandLine;
using Rombadil.Assembler;

var inputArgument = new Argument<FileInfo>("input", "Input source file");
var outputOption = new Option<FileInfo>(["--output", "-o"], "Output binary file");

var rootCommand = new RootCommand("Rombadil Assembler (rmdl)")
{
    inputArgument,
    outputOption
};

int exitCode = 1;

rootCommand.SetHandler((FileInfo input, FileInfo? output) =>
{
    if (!input.Exists)
    {
        PrintError($"{input.Name}: ", "File not found");
        return;
    }

    string[] source = File.ReadAllLines(input.FullName);
    byte[]? binary;

    try
    {
        binary = new Assembler6502().Assemble(source);
    }
    catch (Assembler6502Exception e)
    {
        PrintError($"{input.Name}:{e.Line + 1}: ", e.Error.TrimEnd('.'));
        return;
    }

    string dir = Path.GetDirectoryName(input.FullName)!;
    string file = $"{Path.GetFileNameWithoutExtension(input.FullName)}.rmdl";

    if (output?.DirectoryName != null)
    {
        dir = output.DirectoryName;
        file = output.FullName;
    }

    Directory.CreateDirectory(dir);
    File.WriteAllBytes(file, binary);
    exitCode = 0;

}, inputArgument, outputOption);

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
