namespace Rombadil.Cpu.Emulator;

internal readonly ref struct CpuEmulatorIllegalExecutor(CpuEmulatorState cpu, CpuAddressingMode mode)
{
    internal void Nop(CpuAddressingMode mode) => cpu.AddrIllegal(CpuEmulatorIllegalInstruction.NOP, mode);

    internal void Lax(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.LAX, mode);
        cpu.AC = value;
        cpu.X = value;
    }

    internal void Sax(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.SAX, mode);
        value = (byte)(cpu.AC & cpu.X);
    }

    internal void Sbc() => new CpuEmulatorExecutor(cpu, mode).Sbc();

    internal void Dcp(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.DCP, mode);
        value--;
        cpu.Compare(cpu.AC, value);
    }

    internal void Isb(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.ISB, mode);
        value++;
        cpu.AC = cpu.SubWithBorrow(value);
    }

    internal void Slo(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.SLO, mode);
        value = cpu.ShiftLeft(value);
        cpu.AC |= value;
    }

    internal void Rla(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.RLA, mode);
        value = cpu.RotateLeft(value);
        cpu.AC &= value;
    }

    internal void Sre(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.SRE, mode);
        value = cpu.ShiftRight(value);
        cpu.AC ^= value;
    }

    internal void Rra(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddrIllegal(CpuEmulatorIllegalInstruction.RRA, mode);
        value = cpu.RotateRight(value);
        cpu.AC = cpu.AddWithCarry(value);
    }
}
