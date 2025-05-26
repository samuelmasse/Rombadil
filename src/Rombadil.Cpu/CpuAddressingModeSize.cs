namespace Rombadil.Cpu;

public static class CpuAddressingModeSize
{
    public static int Get(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Indirect ||
            mode == CpuAddressingMode.Absolute ||
            mode == CpuAddressingMode.AbsoluteX ||
            mode == CpuAddressingMode.AbsoluteY)
            return 2;
        else if (mode == CpuAddressingMode.Implied || mode == CpuAddressingMode.Accumulator)
            return 0;
        else return 1;
    }
}
