namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorState
{
    private CpuEmulatorRegisters reg;
    private long cycles;

    public ref CpuEmulatorRegisters Reg => ref reg;
    public ref long Cycles => ref cycles;

    public ref ushort PC => ref reg.PC;
    public ref byte AC => ref reg.AC;
    public ref byte X => ref reg.X;
    public ref byte Y => ref reg.Y;
    public ref CpuStatus SR => ref reg.SR;
    public ref byte SP => ref reg.SP;

    public bool Carry
    {
        get => HasFlag(CpuStatus.Carry);
        set => SetFlag(CpuStatus.Carry, value);
    }

    public bool Zero
    {
        get => HasFlag(CpuStatus.Zero);
        set => SetFlag(CpuStatus.Zero, value);
    }

    public bool Interrupt
    {
        get => HasFlag(CpuStatus.Interrupt);
        set => SetFlag(CpuStatus.Interrupt, value);
    }

    public bool Decimal
    {
        get => HasFlag(CpuStatus.Decimal);
        set => SetFlag(CpuStatus.Decimal, value);
    }

    public bool Break
    {
        get => HasFlag(CpuStatus.Break);
        set => SetFlag(CpuStatus.Break, value);
    }

    public bool Unused
    {
        get => HasFlag(CpuStatus.Unused);
        set => SetFlag(CpuStatus.Unused, value);
    }

    public bool Overflow
    {
        get => HasFlag(CpuStatus.Overflow);
        set => SetFlag(CpuStatus.Overflow, value);
    }

    public bool Negative
    {
        get => HasFlag(CpuStatus.Negative);
        set => SetFlag(CpuStatus.Negative, value);
    }

    private void SetFlag(CpuStatus flag, bool on) => reg.SR = on ? reg.SR | flag : reg.SR & ~flag;
    private bool HasFlag(CpuStatus flag) => (reg.SR & flag) != 0;
}
