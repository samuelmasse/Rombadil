using Rombadil.Assembler;
using Rombadil.Cpu.Emulator;

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

var prg = new Assembler6502().Assemble(source.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));

var bytes = new byte[0x10000];
prg.CopyTo(bytes, 0);

var map = new ushort[bytes.Length];
for (int i = 0; i < map.Length; i++)
    map[i] = (ushort)i;

var state = new CpuEmulatorState();
var memory = new CpuEmulatorMemory(bytes, map);
var cpu = new CpuEmulator6502(state, memory);
var logger = new CpuEmulatorLogger(state, memory, cpu);
cpu.Reset(0);

for (int i = 0; i < 8; i++)
{
    string msg = logger.Log();
    Console.WriteLine($"{i + 1,-8} {msg}");

    cpu.Step();
}
