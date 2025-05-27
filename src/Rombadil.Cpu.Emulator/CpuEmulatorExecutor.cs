namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorExecutor(CpuEmulatorState cpu)
{
    internal void Adc(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.ADC, mode);
        cpu.AC = cpu.AddWithCarry(value);
    }

    internal void And(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.AND, mode);
        cpu.AC &= value;
    }

    internal void Asl(CpuAddressingMode mode)
    {
        var (addr, value) = cpu.AccAddr(CpuInstruction.ASL, mode);
        value = cpu.ShiftLeft(value);
        cpu.WriteAccAddr(mode, addr, value);
    }

    internal void Bit(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.BIT, mode);
        cpu.SetFlag(CpuStatus.Zero, (cpu.AC & value) == 0);
        cpu.SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
        cpu.SetFlag(CpuStatus.Overflow, (value & 0b0100_0000) != 0);
    }

    internal void Bpl() => cpu.Branch(CpuInstruction.BPL, !cpu.HasFlag(CpuStatus.Negative));
    internal void Bmi() => cpu.Branch(CpuInstruction.BMI, cpu.HasFlag(CpuStatus.Negative));

    internal void Bvc() => cpu.Branch(CpuInstruction.BVC, !cpu.HasFlag(CpuStatus.Overflow));
    internal void Bvs() => cpu.Branch(CpuInstruction.BVS, cpu.HasFlag(CpuStatus.Overflow));

    internal void Bcc() => cpu.Branch(CpuInstruction.BCC, !cpu.HasFlag(CpuStatus.Carry));
    internal void Bcs() => cpu.Branch(CpuInstruction.BCS, cpu.HasFlag(CpuStatus.Carry));

    internal void Bne() => cpu.Branch(CpuInstruction.BNE, !cpu.HasFlag(CpuStatus.Zero));
    internal void Beq() => cpu.Branch(CpuInstruction.BEQ, cpu.HasFlag(CpuStatus.Zero));

    internal void Brk()
    {
        cpu.PC++;

        cpu.PushWord(cpu.PC);
        cpu.Push((byte)(cpu.SR | CpuStatus.Break | CpuStatus.Unused));
        cpu.SetFlag(CpuStatus.Interrupt, true);

        cpu.PC = cpu.ReadWord(0xFFFE);
        cpu.Tick(CpuInstruction.BRK);
    }

    internal void Cmp(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.CMP, mode);
        cpu.Compare(cpu.AC, value);
    }

    internal void Cpx(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.CPX, mode);
        cpu.Compare(cpu.X, value);
    }

    internal void Cpy(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.CPY, mode);
        cpu.Compare(cpu.Y, value);
    }

    internal void Dec(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddr(CpuInstruction.DEC, mode);
        value--;
        cpu.SetZN(value);
    }

    internal void Eor(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.EOR, mode);
        cpu.AC ^= value;
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
        ref byte value = ref cpu.ReadAddr(CpuInstruction.INC, mode);
        value++;
        cpu.SetZN(value);
    }

    internal void Jmp(CpuAddressingMode mode)
    {
        ushort addr = cpu.Addr(CpuInstruction.JMP, mode);
        cpu.PC = addr;
    }

    internal void Jsr()
    {
        ushort target = cpu.Addr(CpuInstruction.JSR, CpuAddressingMode.Absolute);
        cpu.PushWord((ushort)(cpu.PC - 1));
        cpu.PC = target;
    }

    internal void Lda(CpuAddressingMode mode) => cpu.AC = cpu.ReadAddr(CpuInstruction.LDA, mode);
    internal void Ldx(CpuAddressingMode mode) => cpu.X = cpu.ReadAddr(CpuInstruction.LDX, mode);
    internal void Ldy(CpuAddressingMode mode) => cpu.Y = cpu.ReadAddr(CpuInstruction.LDY, mode);

    internal void Lsr(CpuAddressingMode mode)
    {
        var (addr, value) = cpu.AccAddr(CpuInstruction.LSR, mode);
        value = cpu.ShiftRight(value);
        cpu.WriteAccAddr(mode, addr, value);
    }

    internal void Nop() => cpu.Tick(CpuInstruction.NOP);

    internal void Ora(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.ORA, mode);
        cpu.AC |= value;
    }

    internal void Tax()
    {
        cpu.Tick(CpuInstruction.TAX);
        cpu.X = cpu.AC;
    }

    internal void Txa()
    {
        cpu.Tick(CpuInstruction.TXA);
        cpu.AC = cpu.X;
    }

    internal void Tay()
    {
        cpu.Tick(CpuInstruction.TAY);
        cpu.Y = cpu.AC;
    }

    internal void Tya()
    {
        cpu.Tick(CpuInstruction.TYA);
        cpu.AC = cpu.Y;
    }

    internal void Dex()
    {
        cpu.Tick(CpuInstruction.DEX);
        cpu.X--;
    }

    internal void Inx()
    {
        cpu.Tick(CpuInstruction.INX);
        cpu.X++;
    }

    internal void Dey()
    {
        cpu.Tick(CpuInstruction.DEY);
        cpu.Y--;
    }

    internal void Iny()
    {
        cpu.Tick(CpuInstruction.INY);
        cpu.Y++;
    }

    internal void Rol(CpuAddressingMode mode)
    {
        var (addr, value) = cpu.AccAddr(CpuInstruction.ROL, mode);
        value = cpu.RotateLeft(value);
        cpu.WriteAccAddr(mode, addr, value);
    }

    internal void Ror(CpuAddressingMode mode)
    {
        var (addr, value) = cpu.AccAddr(CpuInstruction.ROR, mode);
        value = cpu.RotateRight(value);
        cpu.WriteAccAddr(mode, addr, value);
    }

    internal void Rti()
    {
        byte flags = cpu.Pop();
        cpu.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        cpu.PC = cpu.PopWord();
        cpu.Tick(CpuInstruction.RTI);
    }

    internal void Rts()
    {
        cpu.PC = (ushort)(cpu.PopWord() + 1);
        cpu.Tick(CpuInstruction.RTS);
    }

    internal void Sbc(CpuAddressingMode mode)
    {
        byte value = cpu.ReadAddr(CpuInstruction.SBC, mode);
        cpu.AC = cpu.SubWithBorrow(value);
    }

    internal void Sta(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddr(CpuInstruction.STA, mode);
        value = cpu.AC;
    }

    internal void Txs()
    {
        cpu.Tick(CpuInstruction.TXS);
        cpu.SP = cpu.X;
    }

    internal void Tsx()
    {
        cpu.Tick(CpuInstruction.TSX);
        cpu.X = cpu.SP;
    }

    internal void Pha()
    {
        cpu.Tick(CpuInstruction.PHA);
        cpu.Push(cpu.AC);
    }

    internal void Pla()
    {
        cpu.Tick(CpuInstruction.PLA);
        cpu.AC = cpu.Pop();
    }

    internal void Php()
    {
        cpu.Tick(CpuInstruction.PHP);
        byte flags = (byte)(cpu.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);
    }

    internal void Plp()
    {
        cpu.Tick(CpuInstruction.PLP);
        byte flags = cpu.Pop();
        cpu.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
    }

    internal void Stx(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddr(CpuInstruction.STX, mode);
        value = cpu.X;
    }

    internal void Sty(CpuAddressingMode mode)
    {
        ref byte value = ref cpu.ReadAddr(CpuInstruction.STY, mode);
        value = cpu.Y;
    }
}
