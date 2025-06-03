namespace Rombadil.Cpu.Emulator;

public readonly struct CpuEmulatorOperand(CpuEmulatorState s, CpuEmulatorBus b, ushort addr, CpuAddressingMode mode)
{
    public ushort Addr => addr;

    public byte V
    {
        get
        {
            if (mode == CpuAddressingMode.Accumulator)
                return s.AC;
            else return b.Read(addr);
        }
        set
        {
            if (mode == CpuAddressingMode.Accumulator)
                s.AC = value;
            else b.Write(addr, value);
        }
    }
}
