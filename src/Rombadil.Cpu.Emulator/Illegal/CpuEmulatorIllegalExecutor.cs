namespace Rombadil.Cpu.Emulator;

internal ref struct CpuEmulatorIllegalExecutor(CpuEmulatorProcessor p, ref byte v)
{
    private ref byte value = ref v;

    internal void Sax() => value = (byte)(p.AC & p.X);

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

    internal readonly void Sbc() => p.AC = p.SubWithBorrow(value);

    internal readonly void Nop() { }

    internal readonly void Lax()
    {
        p.AC = value;
        p.X = value;
    }
}
