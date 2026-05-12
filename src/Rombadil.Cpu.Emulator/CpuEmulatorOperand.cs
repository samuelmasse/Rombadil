namespace Rombadil.Cpu.Emulator;

internal readonly struct CpuEmulatorOperand(CpuEmulatorState s, CpuEmulatorBus b, ushort addr, CpuAddressingMode mode)
{
    public ushort Addr => addr;

    public byte V
    {
        get => Read();
        set => Write(value);
    }

    internal byte Read()
    {
        if (mode == CpuAddressingMode.Accumulator)
            return s.AC;

        return b.Read(addr);
    }

    internal void Write(byte value)
    {
        if (mode == CpuAddressingMode.Accumulator)
            s.AC = value;
        else b.Write(addr, value);
    }

    internal void DummyWrite(byte value)
    {
        if (mode != CpuAddressingMode.Accumulator)
            b.Write(addr, value);
    }
}
