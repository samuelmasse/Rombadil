using Rombadil.Assembler;
using Rombadil.Cpu.Emulator;

var mem = new byte[80000];
var cpu = new Cpu6502(mem);
var logger = new CpuLogger(mem, cpu);

var assembler = new Assembler6502();

var source =
"""
ADC #$44
ADC $44
ADC $44,X
ADC $4400
ADC $4400,X
ADC $4400,Y
ADC ($44,X)
ADC ($44),Y
""";

source = File.ReadAllText(@"C:\Users\Samuel\Documents\Repos\smb1\game.asm");

var lines = source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

var sw = Stopwatch.StartNew();
var bytes = assembler.Assemble(lines);
sw.Stop();

Console.WriteLine($"took {sw.Elapsed.TotalMilliseconds} ms");

var rbytes = File.ReadAllBytes(@"C:\Users\Samuel\Documents\Repos\smb1\game.nes");

var color = ConsoleColor.Green;
Console.ForegroundColor = color;
int t = 0;

for (int i = 0; i < Math.Max(bytes.Length, rbytes.Length - 0x10); i++)
{
    var b = (i + 8 < bytes.Length) ? bytes[i + 8] : 0;
    var rb = rbytes[i + 0x10];

    var c = b == rb ? ConsoleColor.Green : ConsoleColor.Red;
    if (c != color)
        Console.ForegroundColor = c;

    Console.Write($"{b:X2} ");
    t++;
    if (t % 16 == 0)
        Console.WriteLine();
}

Console.ResetColor();
Console.WriteLine("Assembled output:");
Console.WriteLine(string.Join(" ", bytes.Select(b => $"{b:X2}")));

byte[] program = [
    0xA9, 0x01,       // LDA #$01
    0x8D, 0x00, 0x02, // STA $0200
    0xA9, 0x05,       // LDA #$05
    0x8D, 0x01, 0x02, // STA $0201
    0xAA,             // TAX
    0xE8,             // INX
    0xCA,             // DEX
    0xCA,             // DEX
    0xCA,             // DEX
    0xCA,             // DEX
    0xCA,             // DEX
    0xCA,             // DEX
    0xF0, 0x0F        // BEQ something
];

ushort start = 5000;

mem[0xFFFC] = (byte)(start & 0xFF);
mem[0xFFFD] = (byte)(start >> 8);

program.AsSpan().CopyTo(mem.AsSpan()[start..]);

cpu.Reset();

for (int i = 0; i < 32; i++)
    logger.Step();

Console.WriteLine($"Should be 1 : {mem[0x0200]}");
Console.WriteLine($"Should be 5 : {mem[0x0201]}");
