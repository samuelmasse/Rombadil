namespace Rombadil.Cpu.Emulator;

internal ref struct CpuEmulatorIllegalExecutor(CpuEmulatorHelper cpu, CpuEmulatorState s, CpuEmulatorProcessor p, ushort addr, ref byte cvalue)
{
    private ref byte value = ref cvalue;

    internal void Sax() => value = (byte)(p.AC & p.X);
    internal void Sbc() => new CpuEmulatorExecutor(cpu, s, p, addr, ref value).Sbc();

    internal void Dcp()
    {
        value--;
        p.Compare(p.AC, value);
    }

    internal void Isb()
    {
        value++;
        p.AC = p.SubWithBorrow(value);
    }

    internal void Slo()
    {
        value = p.ShiftLeft(value);
        p.AC |= value;
    }

    internal void Rla()
    {
        value = p.RotateLeft(value);
        p.AC &= value;
    }

    internal void Sre()
    {
        value = p.ShiftRight(value);
        p.AC ^= value;
    }

    internal void Rra()
    {
        value = p.RotateRight(value);
        p.AC = p.AddWithCarry(value);
    }

    internal readonly void Nop() { }

    internal readonly void Lax()
    {
        p.AC = value;
        p.X = value;
    }
}
