namespace Rombadil.Cpu.Emulator;

internal ref struct CpuEmulatorExecutor(CpuEmulatorHelper cpu, CpuEmulatorState s, CpuEmulatorProcessor p, ushort addr, ref byte cvalue)
{
    private ref byte value = ref cvalue;

    internal void Asl() => value = p.ShiftLeft(value);
    internal void Lsr() => value = p.ShiftRight(value);
    internal void Rol() => value = p.RotateLeft(value);
    internal void Ror() => value = p.RotateRight(value);
    internal void Sta() => value = p.AC;
    internal void Stx() => value = p.X;
    internal void Sty() => value = p.Y;

    internal void Dec()
    {
        value--;
        p.SetZN(value);
    }

    internal void Inc()
    {
        value++;
        p.SetZN(value);
    }

    internal readonly void Adc() => p.AC = p.AddWithCarry(value);
    internal readonly void And() => p.AC &= value;
    internal readonly void Bpl() => cpu.Branch(!s.Negative);
    internal readonly void Bmi() => cpu.Branch(s.Negative);
    internal readonly void Bvc() => cpu.Branch(!s.Overflow);
    internal readonly void Bvs() => cpu.Branch(s.Overflow);
    internal readonly void Bcc() => cpu.Branch(!s.Carry);
    internal readonly void Bcs() => cpu.Branch(s.Carry);
    internal readonly void Bne() => cpu.Branch(!s.Zero);
    internal readonly void Beq() => cpu.Branch(s.Zero);
    internal readonly void Cmp() => p.Compare(p.AC, value);
    internal readonly void Cpx() => p.Compare(p.X, value);
    internal readonly void Cpy() => p.Compare(p.Y, value);
    internal readonly void Eor() => p.AC ^= value;
    internal readonly void Clc() => s.Carry = false;
    internal readonly void Sec() => s.Carry = true;
    internal readonly void Cli() => s.Interrupt = false;
    internal readonly void Sei() => s.Interrupt = true;
    internal readonly void Clv() => s.Overflow = false;
    internal readonly void Cld() => s.Decimal = false;
    internal readonly void Sed() => s.Decimal = true;
    internal readonly void Jmp() => s.PC = addr;
    internal readonly void Lda() => p.AC = value;
    internal readonly void Ldx() => p.X = value;
    internal readonly void Ldy() => p.Y = value;
    internal readonly void Nop() { }
    internal readonly void Ora() => p.AC |= value;
    internal readonly void Tax() => p.X = p.AC;
    internal readonly void Txa() => p.AC = p.X;
    internal readonly void Tay() => p.Y = p.AC;
    internal readonly void Tya() => p.AC = p.Y;
    internal readonly void Dex() => p.X--;
    internal readonly void Inx() => p.X++;
    internal readonly void Dey() => p.Y--;
    internal readonly void Iny() => p.Y++;
    internal readonly void Rts() => s.PC = (ushort)(cpu.PopWord() + 1);
    internal readonly void Sbc() => p.AC = p.SubWithBorrow(value);
    internal readonly void Txs() => s.SP = p.X;
    internal readonly void Tsx() => p.X = s.SP;
    internal readonly void Pha() => cpu.Push(p.AC);
    internal readonly void Pla() => p.AC = cpu.Pop();

    internal readonly void Bit()
    {
        s.Zero = (p.AC & value) == 0;
        s.Negative = (value & 0b1000_0000) != 0;
        s.Overflow = (value & 0b0100_0000) != 0;
    }

    internal readonly void Brk()
    {
        s.PC++;
        cpu.PushWord(s.PC);
        cpu.Push((byte)(s.SR | CpuStatus.Break | CpuStatus.Unused));
        s.Interrupt = true;
        s.PC = cpu.ReadWord(0xFFFE);
    }

    internal readonly void Jsr()
    {
        cpu.PushWord((ushort)(s.PC - 1));
        s.PC = addr;
    }

    internal readonly void Rti()
    {
        byte flags = cpu.Pop();
        s.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        s.PC = cpu.PopWord();
    }

    internal readonly void Php()
    {
        byte flags = (byte)(s.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);
    }

    internal readonly void Plp()
    {
        byte flags = cpu.Pop();
        s.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
    }
}
