using Rombadil.Cpu.Emulator;

var mem = new byte[80000];
var cpu = new CpuEmulator6502(mem);
var logger = new CpuEmulatorLogger(mem, cpu);

var nestest = File.ReadAllBytes("nestest.nes");
var nestestlog = File.ReadAllLines("nestest.log");

for (int i = 0; i <= 0x17; i++)
    mem[0x4000 + i] = 0xFF;

var prg = nestest.AsSpan().Slice(0x10, 0x4000);

prg.CopyTo(mem.AsSpan()[0x8000..]);
prg.CopyTo(mem.AsSpan()[0xC000..]);

cpu.Reset(0xC000);

for (int i = 0; i < nestestlog.Length; i++)
{
    string log = logger.Log();

    if (nestestlog[i] != log)
    {
        Console.WriteLine($"{i + 1,-8} Expected : {nestestlog[i]}");
        Console.WriteLine($"{i + 1,-8} But got  : {log}");
        return;
    }

    Console.WriteLine($"{i + 1,-8} {log}");

    cpu.Step();
}
