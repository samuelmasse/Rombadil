namespace Rombadil.Cpu.Emulator;

internal ref struct CpuEmulatorIllegalExecutor(CpuEmulatorState cpu, ushort addr, ref byte cvalue)
{
    private ref byte value = ref cvalue;

    internal void Sax() => value = (byte)(cpu.AC & cpu.X);
    internal void Sbc() => new CpuEmulatorExecutor(cpu, addr, ref value).Sbc();

    internal void Dcp()
    {
        value--;
        cpu.Compare(cpu.AC, value);
    }

    internal void Isb()
    {
        value++;
        cpu.AC = cpu.SubWithBorrow(value);
    }

    internal void Slo()
    {
        value = cpu.ShiftLeft(value);
        cpu.AC |= value;
    }

    internal void Rla()
    {
        value = cpu.RotateLeft(value);
        cpu.AC &= value;
    }

    internal void Sre()
    {
        value = cpu.ShiftRight(value);
        cpu.AC ^= value;
    }

    internal void Rra()
    {
        value = cpu.RotateRight(value);
        cpu.AC = cpu.AddWithCarry(value);
    }

    internal readonly void Nop() { }

    internal readonly void Lax()
    {
        cpu.AC = value;
        cpu.X = value;
    }
}
