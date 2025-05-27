namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorIllegalExecutor(CpuEmulatorState cpu, CpuEmulatorExecutor exec)
{
    internal void Nop(CpuAddressingMode mode)
    {
        cpu.AddrIllegal(CpuEmulatorIllegalInstruction.NOP, mode);
    }

    internal void Lax(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.LAX, mode);
        byte value = cpu.Mem[addr];
        cpu.Reg.AC = value;
        cpu.Reg.X = value;
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Sax(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.SAX, mode);
        byte value = (byte)(cpu.Reg.AC & cpu.Reg.X);
        cpu.Mem[addr] = value;
    }

    internal void Sbc()
    {
        exec.Sbc(CpuAddressingMode.Immediate);
    }

    internal void Dcp(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.DCP, mode);

        byte decremented = (byte)(cpu.Mem[addr] - 1);
        cpu.Mem[addr] = decremented;

        byte acc = cpu.Reg.AC;
        byte result = (byte)(acc - decremented);

        cpu.SetFlag(CpuStatus.Carry, acc >= decremented);
        cpu.SetFlag(CpuStatus.Zero, result == 0);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Isb(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.ISB, mode);

        // INC: increment memory and write back
        byte value = (byte)(cpu.Mem[addr] + 1);
        cpu.Mem[addr] = value;

        // SBC logic (A - value - !C)
        byte inverted = (byte)(value ^ 0xFF);
        ushort sum = (ushort)(cpu.Reg.AC + inverted + (cpu.Reg.SR.HasFlag(CpuStatus.Carry) ? 1 : 0));

        byte result = (byte)sum;

        // Set Carry: no borrow occurred
        cpu.SetFlag(CpuStatus.Carry, sum > 0xFF);

        // Set Overflow: sign change mismatch between A and result, but not with inverted
        bool overflow = ((cpu.Reg.AC ^ result) & (cpu.Reg.AC ^ inverted) & 0x80) != 0;
        cpu.SetFlag(CpuStatus.Overflow, overflow);

        cpu.Reg.AC = result;
        cpu.UpdateZeroNegativeFlags(result);
    }

    internal void Slo(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.SLO, mode);

        byte value = cpu.Mem[addr];
        cpu.SetFlag(CpuStatus.Carry, (value & 0b1000_0000) != 0);

        byte shifted = (byte)(value << 1);
        cpu.Mem[addr] = shifted;

        cpu.Reg.AC |= shifted;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Rla(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.RLA, mode);

        byte value = cpu.Mem[addr];
        bool carryIn = cpu.Reg.SR.HasFlag(CpuStatus.Carry);
        bool carryOut = (value & 0b1000_0000) != 0;

        byte rotated = (byte)((value << 1) | (carryIn ? 1 : 0));
        cpu.Mem[addr] = rotated;

        cpu.SetFlag(CpuStatus.Carry, carryOut);

        cpu.Reg.AC &= rotated;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Sre(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.SRE, mode);

        byte value = cpu.Mem[addr];
        bool carryOut = (value & 0b0000_0001) != 0;

        byte shifted = (byte)(value >> 1);
        cpu.Mem[addr] = shifted;

        cpu.SetFlag(CpuStatus.Carry, carryOut);

        cpu.Reg.AC ^= shifted;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Rra(CpuAddressingMode mode)
    {
        ushort addr = cpu.AddrIllegal(CpuEmulatorIllegalInstruction.RRA, mode);

        byte value = cpu.Mem[addr];
        bool newCarry = (value & 0b0000_0001) != 0;

        byte rotated = (byte)((value >> 1) | (cpu.Reg.SR.HasFlag(CpuStatus.Carry) ? 0x80 : 0));
        cpu.Mem[addr] = rotated;

        // Set carry first (used immediately below)
        cpu.SetFlag(CpuStatus.Carry, newCarry);

        // Use updated carry flag as carry-in
        int carryIn = cpu.Reg.SR.HasFlag(CpuStatus.Carry) ? 1 : 0;

        int a = cpu.Reg.AC;
        int m = rotated;
        int sum = a + m + carryIn;

        cpu.Reg.AC = (byte)sum;
        cpu.SetFlag(CpuStatus.Carry, sum > 0xFF);
        cpu.SetFlag(CpuStatus.Overflow, (~(a ^ m) & (a ^ sum) & 0x80) != 0);
        cpu.UpdateZeroNegativeFlags((byte)sum);
    }

}
