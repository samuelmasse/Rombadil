namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorState
{
    private CpuEmulatorRegisters reg;
    private long cycles;

    internal ref CpuEmulatorRegisters Reg => ref reg;
    internal ref long Cycles => ref cycles;

    internal ref ushort PC => ref reg.PC;
    internal ref byte AC => ref reg.AC;
    internal ref byte X => ref reg.X;
    internal ref byte Y => ref reg.Y;
    internal ref CpuStatus SR => ref reg.SR;
    internal ref byte SP => ref reg.SP;

    internal void SetFlag(CpuStatus flag, bool on) => reg.SR = on ? reg.SR | flag : reg.SR & ~flag;
    internal bool HasFlag(CpuStatus flag) => (reg.SR & flag) != 0;
}
