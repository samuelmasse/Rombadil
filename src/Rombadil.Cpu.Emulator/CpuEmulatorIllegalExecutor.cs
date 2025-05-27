namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorIllegalExecutor(CpuEmulatorState cpu, CpuEmulatorExecutor exec)
{
    internal void Nop(CpuAddressingMode mode) => cpu.AddrIllegal(CpuEmulatorIllegalInstruction.NOP, mode);

    internal void Lax(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.LAX, mode);
        byte value = cpu.Mem[addr];
        cpu.Reg.AC = value;
        cpu.Reg.X = value;
        cpu.SetZN(value);
    }

    internal void Sax(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.SAX, mode);
        byte value = (byte)(cpu.Reg.AC & cpu.Reg.X);
        cpu.Mem[addr] = value;
    }

    internal void Sbc() => exec.Sbc(CpuAddressingMode.Immediate);

    internal void Dcp(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.DCP, mode);
        ref byte value =ref cpu.Mem[addr];
        value--;
        cpu.Compare(cpu.Reg.AC, value);
    }

    internal void Isb(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.ISB, mode);
        ref byte value = ref cpu.Mem[addr];
        value++;
        byte result = cpu.SubWithBorrow(value);
        cpu.SetZN(cpu.Reg.AC = result);
    }

    internal void Slo(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.SLO, mode);
        cpu.Mem[addr] = cpu.ShiftLeft(cpu.Mem[addr]);
        cpu.Reg.AC |= cpu.Mem[addr];
        cpu.SetZN(cpu.Reg.AC);
    }

    internal void Rla(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.RLA, mode);
        cpu.Mem[addr] = cpu.RotateLeft(cpu.Mem[addr]);
        cpu.Reg.AC &= cpu.Mem[addr];
        cpu.SetZN(cpu.Reg.AC);
    }

    internal void Sre(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.SRE, mode);
        cpu.Mem[addr] = cpu.ShiftRight(cpu.Mem[addr]);
        cpu.Reg.AC ^= cpu.Mem[addr];
        cpu.SetZN(cpu.Reg.AC);
    }

    internal void Rra(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.RRA, mode);

        byte rotated = cpu.RotateRight(cpu.Mem[addr]);
        cpu.Mem[addr] = rotated;

        byte result = cpu.AddWithCarry(rotated);
        cpu.Reg.AC = result;
        cpu.SetZN(result);
    }
}
