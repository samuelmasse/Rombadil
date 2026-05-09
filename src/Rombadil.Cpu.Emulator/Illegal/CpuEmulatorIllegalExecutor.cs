namespace Rombadil.Cpu.Emulator;

internal readonly struct CpuEmulatorIllegalExecutor(CpuEmulatorState s, CpuEmulatorBus b, CpuEmulatorProcessor p, CpuEmulatorOperand op)
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

    internal void Aac()
    {
        p.AC = (byte)(s.AC & op.V);
        s.Carry = (s.AC & 0x80) != 0;
    }

    internal void Asr()
    {
        byte temp = (byte)(s.AC & op.V);
        s.Carry = (temp & 0x01) != 0;
        p.AC = (byte)(temp >> 1);
    }

    internal void Arr()
    {
        byte temp = (byte)(s.AC & op.V);
        byte result = (byte)((temp >> 1) | (s.Carry ? 0x80 : 0));
        p.AC = result;
        s.Carry = (result & 0x40) != 0;
        s.Overflow = ((result >> 5) & 1) != ((result >> 6) & 1);
    }

    internal void Atx()
    {
        byte v = op.V;
        s.AC = v;
        p.X = v;
    }

    internal void Axs()
    {
        int temp = (s.AC & s.X) - op.V;
        s.Carry = temp >= 0;
        p.X = (byte)temp;
    }

    internal void Xaa()
    {
        byte v = (byte)((s.AC | 0xEE) & s.X & op.V);
        s.AC = v;
        p.SetZN(v);
    }

    internal void Lar()
    {
        byte v = (byte)(op.V & s.SP);
        s.AC = v;
        s.X = v;
        s.SP = v;
        p.SetZN(v);
    }

    internal void Axa(ushort baseAddr) => StoreHighMasked(baseAddr, (byte)(s.AC & s.X));
    internal void Sxa(ushort baseAddr) => StoreHighMasked(baseAddr, s.X);
    internal void Sya(ushort baseAddr) => StoreHighMasked(baseAddr, s.Y);

    internal void Xas(ushort baseAddr)
    {
        s.SP = (byte)(s.AC & s.X);
        StoreHighMasked(baseAddr, s.SP);
    }

    private void StoreHighMasked(ushort baseAddr, byte reg)
    {
        byte highPlus1 = (byte)((baseAddr >> 8) + 1);
        byte v = (byte)(reg & highPlus1);
        bool crossed = (baseAddr & 0xFF00) != (op.Addr & 0xFF00);
        ushort target = crossed
            ? (ushort)((v << 8) | (op.Addr & 0xFF))
            : op.Addr;
        b.Write(target, v);
    }
}
