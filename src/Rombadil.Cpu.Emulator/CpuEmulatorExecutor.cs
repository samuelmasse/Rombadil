namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorExecutor(CpuEmulatorState cpu)
{
    internal void Adc(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.ADC, mode);
        byte value = cpu.Mem[addr];

        int carry = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        int sum = cpu.Reg.AC + value + carry;

        cpu.SetFlag(CpuStatus.Carry, sum > 0xFF);

        byte result = (byte)sum;

        bool overflow = ((cpu.Reg.AC ^ result) & (value ^ result) & 0x80) != 0;
        cpu.SetFlag(CpuStatus.Overflow, overflow);

        cpu.Reg.AC = result;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void And(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.AND, mode);
        cpu.Reg.AC &= cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Asl(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.ASL, mode);
        byte value = mode == CpuAddressingMode.Accumulator ? cpu.Reg.AC : cpu.Mem[addr];

        cpu.SetFlag(CpuStatus.Carry, (value & 0x80) != 0);
        value <<= 1;
        cpu.UpdateZeroNegativeFlags(value);

        if (mode == CpuAddressingMode.Accumulator)
            cpu.Reg.AC = value;
        else cpu.Mem[addr] = value;
    }

    internal void Bit(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.BIT, mode);
        byte value = cpu.Mem[addr];

        cpu.SetFlag(CpuStatus.Zero, (cpu.Reg.AC & value) == 0);
        cpu.SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
        cpu.SetFlag(CpuStatus.Overflow, (value & 0b0100_0000) != 0);
    }

    internal void Bpl() => cpu.Branch(CpuInstruction.BPL, (cpu.Reg.SR & CpuStatus.Negative) == 0);
    internal void Bmi() => cpu.Branch(CpuInstruction.BMI, (cpu.Reg.SR & CpuStatus.Negative) != 0);

    internal void Bvc() => cpu.Branch(CpuInstruction.BVC, (cpu.Reg.SR & CpuStatus.Overflow) == 0);
    internal void Bvs() => cpu.Branch(CpuInstruction.BVS, (cpu.Reg.SR & CpuStatus.Overflow) != 0);

    internal void Bcc() => cpu.Branch(CpuInstruction.BCC, (cpu.Reg.SR & CpuStatus.Carry) == 0);
    internal void Bcs() => cpu.Branch(CpuInstruction.BCS, (cpu.Reg.SR & CpuStatus.Carry) != 0);

    internal void Bne() => cpu.Branch(CpuInstruction.BNE, (cpu.Reg.SR & CpuStatus.Zero) == 0);
    internal void Beq() => cpu.Branch(CpuInstruction.BEQ, (cpu.Reg.SR & CpuStatus.Zero) != 0);

    internal void Brk()
    {
        cpu.Reg.PC++;

        cpu.Push((byte)((cpu.Reg.PC >> 8) & 0xFF));
        cpu.Push((byte)(cpu.Reg.PC & 0xFF));

        byte flags = (byte)(cpu.Reg.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);

        cpu.SetFlag(CpuStatus.Interrupt, true);

        ushort vector = (ushort)(cpu.Mem[0xFFFE] | (cpu.Mem[0xFFFF] << 8));
        cpu.Reg.PC = vector;
        cpu.Cycles += 7;
    }

    internal void Cmp(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.CMP, mode);
        byte value = cpu.Mem[addr];

        int result = cpu.Reg.AC - value;

        cpu.SetFlag(CpuStatus.Carry, cpu.Reg.AC >= value);
        cpu.SetFlag(CpuStatus.Zero, cpu.Reg.AC == value);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Cpx(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.CPX, mode);
        byte value = cpu.Mem[addr];

        int result = cpu.Reg.X - value;

        cpu.SetFlag(CpuStatus.Carry, cpu.Reg.X >= value);
        cpu.SetFlag(CpuStatus.Zero, cpu.Reg.X == value);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Cpy(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.CPY, mode);
        byte value = cpu.Mem[addr];

        int result = cpu.Reg.Y - value;

        cpu.SetFlag(CpuStatus.Carry, cpu.Reg.Y >= value);
        cpu.SetFlag(CpuStatus.Zero, cpu.Reg.Y == value);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Dec(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.DEC, mode);
        ref byte value = ref cpu.Mem[addr];

        value--;
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Eor(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.EOR, mode);
        cpu.Reg.AC ^= cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Clc()
    {
        cpu.Tick(CpuInstruction.CLC);
        cpu.SetFlag(CpuStatus.Carry, false);
    }

    internal void Sec()
    {
        cpu.Tick(CpuInstruction.SEC);
        cpu.SetFlag(CpuStatus.Carry, true);
    }

    internal void Cli()
    {
        cpu.Tick(CpuInstruction.CLI);
        cpu.SetFlag(CpuStatus.Interrupt, false);
    }

    internal void Sei()
    {
        cpu.Tick(CpuInstruction.SEI);
        cpu.SetFlag(CpuStatus.Interrupt, true);
    }

    internal void Clv()
    {
        cpu.Tick(CpuInstruction.CLV);
        cpu.SetFlag(CpuStatus.Overflow, false);
    }

    internal void Cld()
    {
        cpu.Tick(CpuInstruction.CLD);
        cpu.SetFlag(CpuStatus.Decimal, false);
    }

    internal void Sed()
    {
        cpu.Tick(CpuInstruction.SED);
        cpu.SetFlag(CpuStatus.Decimal, true);
    }

    internal void Inc(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.INC, mode);
        ref byte value = ref cpu.Mem[addr];

        value++;
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Jmp(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.JMP, mode);
        cpu.Reg.PC = addr;
    }

    internal void Jsr()
    {
        ushort target = cpu.Addr(CpuInstruction.JSR, CpuAddressingMode.Absolute);
        cpu.PushWord((ushort)(cpu.Reg.PC - 1));
        cpu.Reg.PC = target;
    }

    internal void Lda(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.LDA, mode);
        cpu.Reg.AC = cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Ldx(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.LDX, mode);
        cpu.Reg.X = cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
    }

    internal void Ldy(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.LDY, mode);
        cpu.Reg.Y = cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
    }

    internal void Lsr(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.LSR, mode);
        byte value = mode == CpuAddressingMode.Accumulator ? cpu.Reg.AC : cpu.Mem[addr];

        cpu.SetFlag(CpuStatus.Carry, (value & 0x01) != 0);
        value >>= 1;
        cpu.SetFlag(CpuStatus.Negative, false);
        cpu.SetFlag(CpuStatus.Zero, value == 0);

        if (mode == CpuAddressingMode.Accumulator)
            cpu.Reg.AC = value;
        else cpu.Mem[addr] = value;
    }

    internal void Nop()
    {
        cpu.Tick(CpuInstruction.NOP);
    }

    internal void Ora(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.ORA, mode);
        cpu.Reg.AC |= cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Tax()
    {
        cpu.Tick(CpuInstruction.TAX);
        cpu.Reg.X = cpu.Reg.AC;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
    }

    internal void Txa()
    {
        cpu.Tick(CpuInstruction.TXA);
        cpu.Reg.AC = cpu.Reg.X;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Tay()
    {
        cpu.Tick(CpuInstruction.TAY);
        cpu.Reg.Y = cpu.Reg.AC;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
    }

    internal void Tya()
    {
        cpu.Tick(CpuInstruction.TYA);
        cpu.Reg.AC = cpu.Reg.Y;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Dex()
    {
        cpu.Tick(CpuInstruction.DEX);
        cpu.Reg.X--;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
    }

    internal void Inx()
    {
        cpu.Tick(CpuInstruction.INX);
        cpu.Reg.X++;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
    }

    internal void Dey()
    {
        cpu.Tick(CpuInstruction.DEY);
        cpu.Reg.Y--;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
    }

    internal void Iny()
    {
        cpu.Tick(CpuInstruction.INY);
        cpu.Reg.Y++;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
    }

    internal void Rol(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.ROL, mode);
        byte value = mode == CpuAddressingMode.Accumulator ? cpu.Reg.AC : cpu.Mem[addr];

        int carryIn = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        bool carryOut = (value & 0x80) != 0;

        value = (byte)((value << 1) | carryIn);

        cpu.SetFlag(CpuStatus.Carry, carryOut);
        cpu.UpdateZeroNegativeFlags(value);

        if (mode == CpuAddressingMode.Accumulator)
            cpu.Reg.AC = value;
        else cpu.Mem[addr] = value;
    }

    internal void Ror(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.ROL, mode);
        byte value = mode == CpuAddressingMode.Accumulator ? cpu.Reg.AC : cpu.Mem[addr];

        int carryIn = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        bool carryOut = (value & 0x01) != 0;

        value = (byte)((value >> 1) | (carryIn << 7));

        cpu.SetFlag(CpuStatus.Carry, carryOut);
        cpu.UpdateZeroNegativeFlags(value);

        if (mode == CpuAddressingMode.Accumulator)
            cpu.Reg.AC = value;
        else cpu.Mem[addr] = value;
    }

    internal void Rti()
    {
        byte flags = cpu.Pop();
        cpu.Reg.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        cpu.Reg.PC = cpu.PopWord();
        cpu.Tick(CpuInstruction.RTI);
    }

    internal void Rts()
    {
        cpu.Reg.PC = (ushort)(cpu.PopWord() + 1);
        cpu.Tick(CpuInstruction.RTS);
    }

    internal void Sbc(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.SBC, mode);
        byte value = cpu.Mem[addr];

        int carry = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        int result = cpu.Reg.AC - value - (1 - carry);

        cpu.SetFlag(CpuStatus.Carry, result >= 0);

        byte resultByte = (byte)result;

        bool overflow = ((cpu.Reg.AC ^ value) & (cpu.Reg.AC ^ resultByte) & 0x80) != 0;
        cpu.SetFlag(CpuStatus.Overflow, overflow);

        cpu.Reg.AC = resultByte;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Sta(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.STA, mode);
        cpu.Mem[addr] = cpu.Reg.AC;
    }

    internal void Txs()
    {
        cpu.Tick(CpuInstruction.TXS);
        cpu.Reg.SP = cpu.Reg.X;
    }

    internal void Tsx()
    {
        cpu.Tick(CpuInstruction.TSX);
        cpu.Reg.X = cpu.Reg.SP;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
    }

    internal void Pha()
    {
        cpu.Tick(CpuInstruction.PHA);
        cpu.Push(cpu.Reg.AC);
    }

    internal void Pla()
    {
        cpu.Tick(CpuInstruction.PLA);
        cpu.Reg.AC = cpu.Pop();
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Php()
    {
        cpu.Tick(CpuInstruction.PHP);
        byte flags = (byte)(cpu.Reg.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);
    }

    internal void Plp()
    {
        cpu.Tick(CpuInstruction.PLP);
        byte flags = cpu.Pop();
        cpu.Reg.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
    }

    internal void Stx(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.STX, mode);
        cpu.Mem[addr] = cpu.Reg.X;
    }

    internal void Sty(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.STY, mode);
        cpu.Mem[addr] = cpu.Reg.Y;
    }
}
