namespace Rombadil.Cpu.Emulator;

public enum CpuEmulatorIllegalInstruction : byte
{
    NOP,
    LAX,
    SAX,
    SBC,
    DCP,
    ISB,
    SLO,
    RLA,
    SRE,
    RRA
}
