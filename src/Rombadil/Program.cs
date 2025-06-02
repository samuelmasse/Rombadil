using Rombadil;

var pixels = new Pixels((256, 240));
using var canvas = new Canvas(pixels);

var rom = File.ReadAllBytes(@"C:\Users\Samuel\Documents\Repos\nes-test-roms\ppu_vbl_nmi\rom_singles\06-suppression.nes");
// var rom = File.ReadAllBytes(@"C:\Users\Samuel\Desktop\NES\Super Mario Bros. (World).nes");


var bytes = new byte[0x10000];
for (int i = 0; i <= 0x17; i++)
    bytes[0x4000 + i] = 0xFF;

var map = new ushort[0x10000];
for (int i = 0; i < map.Length; i++)
    map[i] = (ushort)i;

var prgArea = bytes.AsSpan().Slice(0x8000, 0x8000);
var prgRom = rom.AsSpan().Slice(0x10, 0x8000);
prgRom.CopyTo(prgArea);

var chrRom = rom.AsMemory().Slice(0x8010, 0x2000);

var state = new CpuEmulatorState();
var memory = new CpuEmulatorMemory(bytes, map);
var ppu = new PpuNes(chrRom, pixels);
var controller1 = new NesController();
var controller2 = new NesController();
var bus = new NesMemoryBus(memory, state, ppu, controller1, controller2);
var cpu = new CpuEmulator6502(state, memory, bus);
var logger = new CpuEmulatorLogger(state, memory, cpu);
var sw = Stopwatch.StartNew();

cpu.Reset();
ppu.Reset();

canvas.Render += (delta) =>
{
    sw.Restart();
    NesButtons b = 0;

    if (canvas.IsKeyDown(Keys.LeftControl) && canvas.IsKeyPressed(Keys.R))
    {
        ppu.Reset();
        cpu.Reset();
    }

    if (canvas.IsKeyDown(Keys.S))
        b |= NesButtons.A;
    if (canvas.IsKeyDown(Keys.A))
        b |= NesButtons.B;
    if (canvas.IsKeyDown(Keys.W))
        b |= NesButtons.Start;
    if (canvas.IsKeyDown(Keys.Q))
        b |= NesButtons.Select;
    if (canvas.IsKeyDown(Keys.Up))
        b |= NesButtons.Up;
    if (canvas.IsKeyDown(Keys.Down))
        b |= NesButtons.Down;
    if (canvas.IsKeyDown(Keys.Left))
        b |= NesButtons.Left;
    if (canvas.IsKeyDown(Keys.Right))
        b |= NesButtons.Right;

    controller1.SetButtons(b);

    bool done = false;
    while (!done)
    {
        cpu.Step();

        if (ppu.PendingNmi)
        {
            cpu.Nmi();
            ppu.ClearPendingNmi();
        }

        while (ppu.Cycles < state.Cycles * 3)
        {
            if (ppu.Step())
                done = true;
        }
    }

    // Console.WriteLine($"time {sw.Elapsed.TotalMilliseconds}");
};

canvas.Run();
