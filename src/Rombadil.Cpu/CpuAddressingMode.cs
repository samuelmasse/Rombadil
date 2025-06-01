namespace Rombadil.Cpu;

public enum CpuAddressingMode : byte
{
    Implied,
    Accumulator,
    Immediate,
    Relative,
    Indirect,
    ZeroPage,
    ZeroPageX,
    ZeroPageY,
    Absolute,
    AbsoluteX,
    AbsoluteY,
    IndirectX,
    IndirectY
}
