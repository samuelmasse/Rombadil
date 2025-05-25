using Rombadil.Assembler;
using Rombadil.Cpu.Emulator;

var mem = new byte[80000];
var cpu = new CpuEmulator6502(mem);
var logger = new CpuEmulatorLogger(mem, cpu);
var assembler = new Assembler6502();

var source =
"""
LDA #$01
STA $0200
LDA #$05
STA $0201
TAX
INX
DEX
DEX
DEX
DEX
DEX
DEX
BEQ $0F
""";

var program = assembler.Assemble(source.Split(['\n', '\r']));

ushort start = 5000;
mem[0xFFFC] = (byte)(start & 0xFF);
mem[0xFFFD] = (byte)(start >> 8);
program.AsSpan().CopyTo(mem.AsSpan()[start..]);

cpu.Reset();

for (int i = 0; i < 32; i++)
{
    logger.Log();
    cpu.Step();
}

Console.WriteLine($"Should be 1 : {mem[0x0200]}");
Console.WriteLine($"Should be 5 : {mem[0x0201]}");
