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

    internal bool Carry
    {
        get => HasFlag(CpuStatus.Carry);
        set => SetFlag(CpuStatus.Carry, value);
    }

    internal bool Zero
    {
        get => HasFlag(CpuStatus.Zero);
        set => SetFlag(CpuStatus.Zero, value);
    }

    internal bool Interrupt
    {
        get => HasFlag(CpuStatus.Interrupt);
        set => SetFlag(CpuStatus.Interrupt, value);
    }

    internal bool Decimal
    {
        get => HasFlag(CpuStatus.Decimal);
        set => SetFlag(CpuStatus.Decimal, value);
    }

    internal bool Break
    {
        get => HasFlag(CpuStatus.Break);
        set => SetFlag(CpuStatus.Break, value);
    }

    internal bool Unused
    {
        get => HasFlag(CpuStatus.Unused);
        set => SetFlag(CpuStatus.Unused, value);
    }

    internal bool Overflow
    {
        get => HasFlag(CpuStatus.Overflow);
        set => SetFlag(CpuStatus.Overflow, value);
    }

    internal bool Negative
    {
        get => HasFlag(CpuStatus.Negative);
        set => SetFlag(CpuStatus.Negative, value);
    }

    private void SetFlag(CpuStatus flag, bool on) => reg.SR = on ? reg.SR | flag : reg.SR & ~flag;
    private bool HasFlag(CpuStatus flag) => (reg.SR & flag) != 0;
}
