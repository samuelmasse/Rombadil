using Rombadil;
using Rombadil.Cpu.Emulator;

var pixels = new Pixels((256, 240));
using var canvas = new Canvas(pixels);

var rom = File.ReadAllBytes(@"nestest.nes");

var bytes = new byte[0x10000];
for (int i = 0; i <= 0x17; i++)
    bytes[0x4000 + i] = 0xFF;

var map = new ushort[0x10000];
for (int i = 0; i < map.Length; i++)
    map[i] = (ushort)i;
for (int i = 0; i < 0x4000; i++)
    map[0xC000 + i] = (ushort)(0x8000 + i);

var prgArea = bytes.AsSpan().Slice(0x8000, 0x4000);
var prgRom = rom.AsSpan().Slice(0x10, 0x4000);
prgRom.CopyTo(prgArea);

var chrRom = rom.AsMemory().Slice(0x4010, 0x2000);

var state = new CpuEmulatorState();
var memory = new CpuEmulatorMemory(bytes, map);
var ppu = new PpuNes(chrRom, pixels);
var bus = new NesMemoryBus(memory, ppu);
var cpu = new CpuEmulator6502(state, memory, bus);
var logger = new CpuEmulatorLogger(state, memory, cpu);

cpu.Reset();

RenderChr();

canvas.Render += (delta) =>
{
    long cs = state.Cycles;
    while (state.Cycles - cs < 29780)
    {
        cpu.Step();

        while (ppu.Cycles < state.Cycles * 3)
        {
            if (ppu.Step() && (ppu.Ctrl & 0x80) != 0)
                cpu.Nmi();
        }
    }
};

canvas.Run();

void RenderChr()
{
    int tileCount = chrRom.Length / 16;
    int tilesPerRow = 16;

    for (int tileIndex = 0; tileIndex < tileCount; tileIndex++)
    {
        int tileX = tileIndex % tilesPerRow;
        int tileY = tileIndex / tilesPerRow;

        int tileOffset = tileIndex * 16;
        var plane0 = chrRom.Slice(tileOffset, 8);
        var plane1 = chrRom.Slice(tileOffset + 8, 8);

        for (int y = 0; y < 8; y++)
        {
            byte b0 = plane0.Span[y];
            byte b1 = plane1.Span[y];

            for (int x = 0; x < 8; x++)
            {
                int bit = 7 - x;
                int lowBit = (b0 >> bit) & 1;
                int highBit = (b1 >> bit) & 1;
                int color = (highBit << 1) | lowBit;

                byte c = color switch
                {
                    0 => 0,
                    1 => 85,
                    2 => 170,
                    3 => 255,
                    _ => 0
                };

                int px = tileX * 8 + x;
                int py = tileY * 8 + y;

                if (px < 256 && py < 240)
                {
                    pixels[(py * pixels.Size.X + px) * 3 + 0] = c;
                    pixels[(py * pixels.Size.X + px) * 3 + 1] = c;
                    pixels[(py * pixels.Size.X + px) * 3 + 2] = c;
                }
            }
        }
    }
}

/*
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
