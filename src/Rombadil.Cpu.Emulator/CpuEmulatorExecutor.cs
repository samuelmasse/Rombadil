namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorExecutor(CpuEmulatorState cpu)
{
    internal void Adc(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.ADC, mode);
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
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.AND, mode);
        cpu.Reg.AC &= cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Asl(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            cpu.SetFlag(CpuStatus.Carry, (cpu.Reg.AC & 0x80) != 0);
            cpu.Reg.AC <<= 1;
            cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
            cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.ASL, mode).Cycles;
            return;
        }

        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.ASL, mode);
        ref byte value = ref cpu.Mem[addr];

        cpu.SetFlag(CpuStatus.Carry, (value & 0x80) != 0);
        value <<= 1;
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Bit(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.BIT, mode);
        byte value = cpu.Mem[addr];

        cpu.SetFlag(CpuStatus.Zero, (cpu.Reg.AC & value) == 0);
        cpu.SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
        cpu.SetFlag(CpuStatus.Overflow, (value & 0b0100_0000) != 0);
    }

    internal void Bpl() => Branch(CpuInstruction.BPL, (cpu.Reg.SR & CpuStatus.Negative) == 0);
    internal void Bmi() => Branch(CpuInstruction.BMI, (cpu.Reg.SR & CpuStatus.Negative) != 0);

    internal void Bvc() => Branch(CpuInstruction.BVC, (cpu.Reg.SR & CpuStatus.Overflow) == 0);
    internal void Bvs() => Branch(CpuInstruction.BVS, (cpu.Reg.SR & CpuStatus.Overflow) != 0);

    internal void Bcc() => Branch(CpuInstruction.BCC, (cpu.Reg.SR & CpuStatus.Carry) == 0);
    internal void Bcs() => Branch(CpuInstruction.BCS, (cpu.Reg.SR & CpuStatus.Carry) != 0);

    internal void Bne() => Branch(CpuInstruction.BNE, (cpu.Reg.SR & CpuStatus.Zero) == 0);
    internal void Beq() => Branch(CpuInstruction.BEQ, (cpu.Reg.SR & CpuStatus.Zero) != 0);

    internal void Brk()
    {
        cpu.Reg.PC++; // Skip padding byte after $00 opcode (even if unused)

        // cpu.Push PC high then low (return address = PC + 1)
        cpu.Push((byte)((cpu.Reg.PC >> 8) & 0xFF));
        cpu.Push((byte)(cpu.Reg.PC & 0xFF));

        // cpu.Push status with Break flag set (bit 4 = 1), bit 5 always set
        byte flags = (byte)(cpu.Reg.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);

        // Set Interrupt disable
        cpu.SetFlag(CpuStatus.Interrupt, true);

        // Jump to vector at $FFFE-$FFFF
        ushort vector = (ushort)(cpu.Mem[0xFFFE] | (cpu.Mem[0xFFFF] << 8));
        cpu.Reg.PC = vector;
        cpu.Cycles += 7;
    }

    internal void Cmp(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.CMP, mode);
        byte value = cpu.Mem[addr];

        int result = cpu.Reg.AC - value;

        cpu.SetFlag(CpuStatus.Carry, cpu.Reg.AC >= value);
        cpu.SetFlag(CpuStatus.Zero, cpu.Reg.AC == value);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Cpx(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.CPX, mode);
        byte value = cpu.Mem[addr];

        int result = cpu.Reg.X - value;

        cpu.SetFlag(CpuStatus.Carry, cpu.Reg.X >= value);
        cpu.SetFlag(CpuStatus.Zero, cpu.Reg.X == value);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Cpy(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.CPY, mode);
        byte value = cpu.Mem[addr];

        int result = cpu.Reg.Y - value;

        cpu.SetFlag(CpuStatus.Carry, cpu.Reg.Y >= value);
        cpu.SetFlag(CpuStatus.Zero, cpu.Reg.Y == value);
        cpu.SetFlag(CpuStatus.Negative, (result & 0x80) != 0);
    }

    internal void Dec(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.DEC, mode);
        ref byte value = ref cpu.Mem[addr];

        value--;
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Eor(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.EOR, mode);
        cpu.Reg.AC ^= cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Clc()
    {
        cpu.SetFlag(CpuStatus.Carry, false);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.CLC, CpuAddressingMode.Implied).Cycles;
    }

    internal void Sec()
    {
        cpu.SetFlag(CpuStatus.Carry, true);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.SEC, CpuAddressingMode.Implied).Cycles;
    }

    internal void Cli()
    {
        cpu.SetFlag(CpuStatus.Interrupt, false);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.CLI, CpuAddressingMode.Implied).Cycles;
    }

    internal void Sei()
    {
        cpu.SetFlag(CpuStatus.Interrupt, true);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.SEI, CpuAddressingMode.Implied).Cycles;
    }

    internal void Clv()
    {
        cpu.SetFlag(CpuStatus.Overflow, false);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.CLV, CpuAddressingMode.Implied).Cycles;
    }

    internal void Cld()
    {
        cpu.SetFlag(CpuStatus.Decimal, false);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.CLD, CpuAddressingMode.Implied).Cycles;
    }

    internal void Sed()
    {
        cpu.SetFlag(CpuStatus.Decimal, true);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.SED, CpuAddressingMode.Implied).Cycles;
    }

    internal void Inc(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.INC, mode);
        ref byte value = ref cpu.Mem[addr];

        value++;
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Jmp(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Absolute)
        {
            cpu.Reg.PC = cpu.ReadWord();
        }
        else if (mode == CpuAddressingMode.Indirect)
        {
            ushort addr = cpu.ReadWord();

            // Emulate JMP ($xxFF) bug — no page carry
            var span = cpu.Mem;
            byte low = span[addr];
            byte high = span[(ushort)((addr & 0xFF00) | ((addr + 1) & 0x00FF))];

            cpu.Reg.PC = (ushort)(low | (high << 8));
        }

        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.JMP, mode).Cycles;
    }

    internal void Jsr()
    {
        ushort target = cpu.ReadWord();

        ushort returnAddr = (ushort)(cpu.Reg.PC - 1);
        cpu.Push((byte)((returnAddr >> 8) & 0xFF));
        cpu.Push((byte)(returnAddr & 0xFF));

        cpu.Reg.PC = target;
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.JSR, CpuAddressingMode.Absolute).Cycles;
    }

    internal void Lda(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.LDA, mode);
        cpu.Reg.AC = cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Ldx(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.LDX, mode);
        cpu.Reg.X = cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
    }

    internal void Ldy(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.LDY, mode);
        cpu.Reg.Y = cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
    }

    internal void Lsr(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            cpu.SetFlag(CpuStatus.Carry, (cpu.Reg.AC & 0x01) != 0);
            cpu.Reg.AC >>= 1;
            cpu.SetFlag(CpuStatus.Negative, false);
            cpu.SetFlag(CpuStatus.Zero, cpu.Reg.AC == 0);

            cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.LSR, mode).Cycles;
            return;
        }

        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.LSR, mode);
        ref byte value = ref cpu.Mem[addr];

        cpu.SetFlag(CpuStatus.Carry, (value & 0x01) != 0);
        value >>= 1;
        cpu.SetFlag(CpuStatus.Negative, false);
        cpu.SetFlag(CpuStatus.Zero, value == 0);
    }

    internal void Nop()
    {
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.NOP, CpuAddressingMode.Implied).Cycles;
    }

    internal void Ora(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.ORA, mode);
        cpu.Reg.AC |= cpu.Mem[addr];
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Tax()
    {
        cpu.Reg.X = cpu.Reg.AC;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.TAX, CpuAddressingMode.Implied).Cycles;
    }

    internal void Txa()
    {
        cpu.Reg.AC = cpu.Reg.X;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.TXA, CpuAddressingMode.Implied).Cycles;
    }

    internal void Tay()
    {
        cpu.Reg.Y = cpu.Reg.AC;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.TAY, CpuAddressingMode.Implied).Cycles;
    }

    internal void Tya()
    {
        cpu.Reg.AC = cpu.Reg.Y;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.TYA, CpuAddressingMode.Implied).Cycles;
    }

    internal void Dex()
    {
        cpu.Reg.X--;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.DEX, CpuAddressingMode.Implied).Cycles;
    }

    internal void Inx()
    {
        cpu.Reg.X++;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.INX, CpuAddressingMode.Implied).Cycles;
    }

    internal void Dey()
    {
        cpu.Reg.Y--;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.DEY, CpuAddressingMode.Implied).Cycles;
    }

    internal void Iny()
    {
        cpu.Reg.Y++;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.Y);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.INY, CpuAddressingMode.Implied).Cycles;
    }

    internal void Rol(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            int carryIn = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
            bool carryOut = (cpu.Reg.AC & 0x80) != 0;

            cpu.Reg.AC = (byte)((cpu.Reg.AC << 1) | carryIn);

            cpu.SetFlag(CpuStatus.Carry, carryOut);
            cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);

            cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.ROL, mode).Cycles;
            return;
        }

        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.ROL, mode);
        ref byte value = ref cpu.Mem[addr];

        int carryInMem = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        bool carryOutMem = (value & 0x80) != 0;

        value = (byte)((value << 1) | carryInMem);

        cpu.SetFlag(CpuStatus.Carry, carryOutMem);
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Ror(CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Implied)
        {
            int carryIn = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
            bool carryOut = (cpu.Reg.AC & 0x01) != 0;

            cpu.Reg.AC = (byte)((cpu.Reg.AC >> 1) | (carryIn << 7));

            cpu.SetFlag(CpuStatus.Carry, carryOut);
            cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);

            cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.ROR, mode).Cycles;
            return;
        }

        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.ROR, mode);
        ref byte value = ref cpu.Mem[addr];

        int carryInMem = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        bool carryOutMem = (value & 0x01) != 0;

        value = (byte)((value >> 1) | (carryInMem << 7));

        cpu.SetFlag(CpuStatus.Carry, carryOutMem);
        cpu.UpdateZeroNegativeFlags(value);
    }

    internal void Rti()
    {
        // Pull status first (flags)
        byte flags = cpu.Pop();
        cpu.Reg.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused); // B is cleared, bit 5 stays set

        // Pull PC low, then high
        byte pcl = cpu.Pop();
        byte pch = cpu.Pop();
        cpu.Reg.PC = (ushort)(pcl | (pch << 8));

        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.RTI, CpuAddressingMode.Implied).Cycles;
    }

    internal void Rts()
    {
        // Pull PC low, then high
        byte pcl = cpu.Pop();
        byte pch = cpu.Pop();
        cpu.Reg.PC = (ushort)((pcl | (pch << 8)) + 1);

        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.RTS, CpuAddressingMode.Implied).Cycles;
    }

    internal void Sbc(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.SBC, mode);
        byte value = cpu.Mem[addr];

        int carry = (cpu.Reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        int result = cpu.Reg.AC - value - (1 - carry);

        // Set Carry: set if no borrow occurred (i.e., A ≥ value + (1 - C))
        cpu.SetFlag(CpuStatus.Carry, result >= 0);

        byte resultByte = (byte)result;

        // Set Overflow: if sign bit flipped incorrectly during subtraction
        bool overflow = ((cpu.Reg.AC ^ value) & (cpu.Reg.AC ^ resultByte) & 0x80) != 0;
        cpu.SetFlag(CpuStatus.Overflow, overflow);

        cpu.Reg.AC = resultByte;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
    }

    internal void Sta(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.STA, mode);
        cpu.Mem[addr] = cpu.Reg.AC;
    }

    internal void Txs()
    {
        cpu.Reg.SP = cpu.Reg.X;
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.TXS, CpuAddressingMode.Implied).Cycles;
    }

    internal void Tsx()
    {
        cpu.Reg.X = cpu.Reg.SP;
        cpu.UpdateZeroNegativeFlags(cpu.Reg.X);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.TSX, CpuAddressingMode.Implied).Cycles;
    }

    internal void Pha()
    {
        cpu.Push(cpu.Reg.AC);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.PHA, CpuAddressingMode.Implied).Cycles;
    }

    internal void Pla()
    {
        cpu.Reg.AC = cpu.Pop();
        cpu.UpdateZeroNegativeFlags(cpu.Reg.AC);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.PLA, CpuAddressingMode.Implied).Cycles;
    }

    internal void Php()
    {
        byte flags = (byte)(cpu.Reg.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.PHP, CpuAddressingMode.Implied).Cycles;
    }

    internal void Plp()
    {
        byte flags = cpu.Pop();
        cpu.Reg.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        cpu.Cycles += CpuEmulatorTimings.Get(CpuInstruction.PLP, CpuAddressingMode.Implied).Cycles;
    }

    internal void Stx(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.STX, mode);
        cpu.Mem[addr] = cpu.Reg.X;
    }

    internal void Sty(CpuAddressingMode mode)
    {
        ushort addr = cpu.ResolveOperandAddressWithCycles(CpuInstruction.STY, mode);
        cpu.Mem[addr] = cpu.Reg.Y;
    }

    private void Branch(CpuInstruction instr, bool condition)
    {
        var timing = CpuEmulatorTimings.Get(instr, CpuAddressingMode.Relative);
        cpu.Cycles += timing.Cycles;

        sbyte offset = (sbyte)cpu.Mem[cpu.Reg.PC++];
        if (!condition)
            return;

        cpu.Cycles += 1;

        ushort originalPC = cpu.Reg.PC;
        cpu.Reg.PC = (ushort)(cpu.Reg.PC + offset);

        if ((originalPC & 0xFF00) != (cpu.Reg.PC & 0xFF00))
            cpu.Cycles += timing.PagePenalty;
    }
}
