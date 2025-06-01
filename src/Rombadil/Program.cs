using Rombadil;

var pixels = new Pixels((256, 240));
using var canvas = new Canvas(pixels);

int s = 0;
canvas.Render += (delta) =>
{
    for (int i = 0; i < 1000; i++)
    {
        s++;
        pixels[s % pixels.Data.Length] = (byte)(s % 255);
        s++;
        pixels[s % pixels.Data.Length] = (byte)(s % 255);
        s++;
        pixels[s % pixels.Data.Length] = (byte)(s % 255);
    }
};

canvas.Run();

/*
var bytes = new byte[0x10000];
for (int i = 0; i <= 0x17; i++)
    bytes[0x4000 + i] = 0xFF;

var map = new ushort[0x10000];
for (int i = 0; i < map.Length; i++)
    map[i] = (ushort)i;
for (int i = 0; i < 0x4000; i++)
    map[0xC000 + i] = (ushort)(0x8000 + i);

var rom = File.ReadAllBytes("nestest.nes");
var prgArea = bytes.AsSpan().Slice(0x8000, 0x4000);
var prgRom = rom.AsSpan().Slice(0x10, 0x4000);
prgRom.CopyTo(prgArea);

var state = new CpuEmulatorState();
var memory = new CpuEmulatorMemory(bytes, map);
var cpu = new CpuEmulator6502(state, memory);
var logger = new CpuEmulatorLogger(state, memory, cpu);
cpu.Reset(0xC000);

var mod =
"""
LDX $00
LDY $00
JMP $C000
""";

var modePrg = new Assembler6502().Assemble(mod.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
for (int i = 0; i < modePrg.Length; i++)
    memory[(ushort)(0xC66E + i)] = modePrg[i];

var stopwatch = Stopwatch.StartNew();
long lastCycles = 0;
long lastElapsedMs = 0;

while (true)
{
    cpu.Step();

    if (state.Cycles % 0x100000 == 0 && stopwatch.ElapsedMilliseconds - lastElapsedMs >= 1000)
    {
        long currentCycles = state.Cycles;
        long elapsedMs = stopwatch.ElapsedMilliseconds - lastElapsedMs;
        long deltaCycles = currentCycles - lastCycles;
        double rate = (deltaCycles * 1000.0) / elapsedMs;

        Console.WriteLine($"{rate:F0} cycles/sec");
        Console.WriteLine(logger.Log());

        lastCycles = currentCycles;
        lastElapsedMs = stopwatch.ElapsedMilliseconds;
    }
}
*/
