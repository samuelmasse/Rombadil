var inputArgument = new Argument<FileInfo>("input", "Input binary file");

var startOption = new Option<ushort>(
    aliases: ["-s", "--start"],
    description: "Start of the program in the binary file",
    parseArgument: (result) => ParseNumber(result, "s"));

var lengthOption = new Option<ushort>(
    aliases: ["-l", "--length"],
    description: "Length of the program in the binary file",
    parseArgument: (result) => ParseNumber(result, "l"));

var memoryLocationOption = new Option<List<ushort>>(
    aliases: ["-m", "--memory-location"],
    description: "Memory location to write the program to",
    parseArgument: (result) =>
    {
        var values = new List<ushort>();

        foreach (var token in result.Tokens)
        {
            if (TryParseNumber(result, token.Value, "m", out var value))
                values.Add(value);
            else return [];
        }

        return values;
    })
{
    Arity = ArgumentArity.OneOrMore,
    AllowMultipleArgumentsPerToken = true
};

var regionMapOption = new Option<List<MemoryRegionRemap>>(
    aliases: ["-r", "--remap"],
    description: "Remap memory region (source,length=target)",
    parseArgument: result =>
    {
        var remaps = new List<MemoryRegionRemap>();

        foreach (var token in result.Tokens)
        {
            try
            {
                var parts = token.Value.Split('=');
                var sourceParts = parts[0].Split(',');
                ushort source = ParseNumberString(sourceParts[0]);
                ushort length = ParseNumberString(sourceParts[1]);
                ushort target = ParseNumberString(parts[1]);
                remaps.Add(new MemoryRegionRemap(source, target, length));
            }
            catch
            {
                result.ErrorMessage = $"Invalid remap '{token.Value}'";
                return [];
            }
        }

        return remaps;
    })
{
    Arity = ArgumentArity.ZeroOrMore,
    AllowMultipleArgumentsPerToken = true
};

var byteOption = new Option<List<MemoryRegionWrite>>(
    aliases: ["-b", "--byte"],
    description: "Write byte values to memory before starting execution (start,length=value)",
    parseArgument: result => ParseWrites(result, "byte"))
{
    Arity = ArgumentArity.ZeroOrMore,
    AllowMultipleArgumentsPerToken = true
};

var wordOption = new Option<List<MemoryRegionWrite>>(
    aliases: ["-w", "--word"],
    description: "Write word values to memory before starting execution (start,length=value)",
    parseArgument: result => ParseWrites(result, "word"))
{
    Arity = ArgumentArity.ZeroOrMore,
    AllowMultipleArgumentsPerToken = true
};

var programCounterOption = new Option<ushort?>(
    aliases: ["-p", "--program-counter"],
    description: "Initial value of the program counter",
    parseArgument: (result) => ParseNumber(result, "p"));

var rootCommand = new RootCommand("Rombadil CPU Emulator (em65)")
{
    inputArgument,
    startOption,
    lengthOption,
    memoryLocationOption,
    programCounterOption,
    regionMapOption,
    byteOption,
    wordOption
};

int exitCode = 1;

rootCommand.SetHandler((input, start, length, mem, pc, remap, byteValues, wordValues) =>
{
    if (!input.Exists)
    {
        PrintError($"{input.Name}: ", "File not found");
        return;
    }

    byte[] rom = File.ReadAllBytes(input.FullName);
    if (length == 0)
        length = (ushort)(rom.Length - start);

    if (start >= rom.Length)
    {
        PrintError($"start ", $"Out of range ({start} is larger than the file size of {rom.Length})");
        return;
    }

    if (start + length > rom.Length)
    {
        PrintError($"length ", $"Out of range ({start}+{length}={start + length} is larger than the file size of {rom.Length})");
        return;
    }

    var bytes = new byte[0x10000];
    if (mem.Count == 0)
        mem = [0];

    var prg = rom.AsSpan().Slice(start, length);
    foreach (var m in mem)
    {
        if (m + length > bytes.Length)
        {
            PrintError($"memory ", $"Out of range ({m}+{length}={m + length} is larger than the memory available {bytes.Length})");
            return;
        }

        prg.CopyTo(bytes.AsSpan().Slice(m, length));
    }

    var map = new ushort[bytes.Length];
    for (int i = 0; i < map.Length; i++)
        map[i] = (ushort)i;

    foreach (var r in remap)
    {
        if (r.SourceStart + r.Length > bytes.Length)
        {
            PrintError($"remap ",
                $"Out of range ({r.SourceStart}+{r.Length}={r.SourceStart + r.Length} is larger than the memory available {bytes.Length})");
            return;
        }

        if (r.TargetStart + r.Length > bytes.Length)
        {
            PrintError($"remap ",
                $"Out of range ({r.TargetStart}+{r.Length}={r.TargetStart + r.Length} is larger than the memory available {bytes.Length})");
            return;
        }

        for (int i = 0; i < r.Length; i++)
            map[r.SourceStart + i] = (ushort)(r.TargetStart + i);
    }

    var state = new CpuEmulatorState();
    var bus = new CpuEmulatorBusMap(bytes, map);
    var cpu = new CpuEmulator6502(state, bus);
    var logger = new CpuEmulatorLogger(state, bus, cpu);

    foreach (var value in byteValues)
    {
        if (value.Start + value.Length > bytes.Length)
        {
            PrintError($"byte ",
                $"Out of range ({value.Start}+{value.Length}={value.Start + value.Length} " +
                $"is larger than the memory available {bytes.Length})");
            return;
        }

        if (value.Value > 0xFF)
        {
            PrintError($"byte ", $"Invalid value {value.Value} cannot be higher than 255");
            return;
        }

        for (int i = 0; i < value.Length; i++)
            bus[(ushort)(value.Start + i)] = (byte)value.Value;
    }

    foreach (var value in wordValues)
    {
        var l = value.Length * 2;
        if (value.Start + l > bytes.Length)
        {
            PrintError($"word ",
                $"Out of range ({value.Start}+{l}={value.Start + l} " +
                $"is larger than the memory available {bytes.Length})");
            return;
        }

        for (int i = 0; i < l; i++)
        {
            bus[(ushort)(value.Start + i * 2)] = (byte)(value.Value & 0xFF);
            bus[(ushort)(value.Start + i * 2 + 1)] = (byte)((value.Value >> 8) & 0xFF);
        }
    }

    cpu.Reset(pc);

    CpuOpcode code;
    do
    {
        Console.WriteLine(logger.Log());
        code = (CpuOpcode)bus[state.PC];
        cpu.Step();
    }
    while (code != CpuOpcode.BRK);

    exitCode = 0;
}, inputArgument, startOption, lengthOption, memoryLocationOption, programCounterOption, regionMapOption, byteOption, wordOption);

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

static List<MemoryRegionWrite> ParseWrites(ArgumentResult result, string type)
{
    var writes = new List<MemoryRegionWrite>();

    foreach (var token in result.Tokens)
    {
        try
        {
            var parts = token.Value.Split('=');
            var sourceParts = parts[0].Split(',');
            ushort start = ParseNumberString(sourceParts[0]);
            ushort length = sourceParts.Length > 1 ? ParseNumberString(sourceParts[1]) : (ushort)1;
            ushort value = ParseNumberString(parts[1]);
            writes.Add(new MemoryRegionWrite(start, value, length));
        }
        catch
        {
            result.ErrorMessage = $"Invalid {type} '{token.Value}'";
            return [];
        }
    }

    return writes;
}

static ushort ParseNumber(ArgumentResult result, string alias)
{
    TryParseNumber(result, result.Tokens[0].Value, alias, out var value);
    return value;
}

static bool TryParseNumber(ArgumentResult result, string input, string alias, out ushort value)
{
    try
    {
        value = ParseNumberString(input);
        return true;
    }
    catch
    {
        value = 0;
        result.ErrorMessage = $"Cannot parse argument '{input}' for option '-{alias}' as expected 'System.UInt16'.";
        return false;
    }
}

static ushort ParseNumberString(string input)
{
    if (input.StartsWith("0x"))
        return Convert.ToUInt16(input[2..], 16);
    if (input.EndsWith('h'))
        return Convert.ToUInt16(input[..^1], 16);
    if (input.StartsWith('$'))
        return Convert.ToUInt16(input[1..], 16);
    return Convert.ToUInt16(input, 10);
}

record struct MemoryRegionRemap(ushort SourceStart, ushort TargetStart, ushort Length);
record struct MemoryRegionWrite(ushort Start, ushort Value, ushort Length);
