namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorState
{
    private CpuEmulatorRegisters reg;
    private long cycles;

    internal ref CpuEmulatorRegisters Reg => ref reg;
    internal ref long Cycles => ref cycles;

    internal void SetFlag(CpuStatus flag, bool on) => reg.SR = on ? reg.SR | flag : reg.SR & ~flag;
    internal bool HasFlag(CpuStatus flag) => (reg.SR & flag) != 0;
}
