namespace Rombadil.Cpu.Emulator;

internal readonly struct CpuEmulatorIllegalExecutor(CpuEmulatorProcessor p, CpuEmulatorOperand op)
{
    internal void Sax() => op.V = (byte)(p.AC & p.X);
    internal void Dcp() => p.Compare(p.AC, --op.V);
    internal void Isb() => p.AC = p.SubWithBorrow(++op.V);

    internal void Slo()
    {
        byte v = p.ShiftLeft(op.V);
        op.V = v;
        p.AC |= v;
    }

    internal void Rla()
    {
        byte v = p.RotateLeft(op.V);
        op.V = v;
        p.AC &= v;
    }

    internal void Sre()
    {
        byte v = p.ShiftRight(op.V);
        op.V = v;
        p.AC ^= v;
    }

    internal void Rra()
    {
        byte v = p.RotateRight(op.V);
        op.V = v;
        p.AC = p.AddWithCarry(v);
    }

    internal void Sbc() => p.AC = p.SubWithBorrow(op.V);

    internal void Nop() { }

    internal void Lax()
    {
        byte v = op.V;
        p.AC = v;
        p.X = v;
    }
}
