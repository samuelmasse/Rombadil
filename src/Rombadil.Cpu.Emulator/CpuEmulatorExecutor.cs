namespace Rombadil.Cpu.Emulator;

internal readonly struct CpuEmulatorExecutor(CpuEmulatorState s, CpuEmulatorMemory m, CpuEmulatorProcessor p, CpuEmulatorOperand op)
{
    internal void Asl() => op.V = p.ShiftLeft(op.V);
    internal void Lsr() => op.V = p.ShiftRight(op.V);
    internal void Rol() => op.V = p.RotateLeft(op.V);
    internal void Ror() => op.V = p.RotateRight(op.V);
    internal void Sta() => op.V = p.AC;
    internal void Stx() => op.V = p.X;
    internal void Sty() => op.V = p.Y;
    internal void Dec() => p.SetZN(--op.V);
    internal void Inc() => p.SetZN(++op.V);
    internal void Adc() => p.AC = p.AddWithCarry(op.V);
    internal void And() => p.AC &= op.V;
    internal void Bpl() => p.Branch(!s.Negative);
    internal void Bmi() => p.Branch(s.Negative);
    internal void Bvc() => p.Branch(!s.Overflow);
    internal void Bvs() => p.Branch(s.Overflow);
    internal void Bcc() => p.Branch(!s.Carry);
    internal void Bcs() => p.Branch(s.Carry);
    internal void Bne() => p.Branch(!s.Zero);
    internal void Beq() => p.Branch(s.Zero);
    internal void Cmp() => p.Compare(p.AC, op.V);
    internal void Cpx() => p.Compare(p.X, op.V);
    internal void Cpy() => p.Compare(p.Y, op.V);
    internal void Eor() => p.AC ^= op.V;
    internal void Clc() => s.Carry = false;
    internal void Sec() => s.Carry = true;
    internal void Cli() => s.Interrupt = false;
    internal void Sei() => s.Interrupt = true;
    internal void Clv() => s.Overflow = false;
    internal void Cld() => s.Decimal = false;
    internal void Sed() => s.Decimal = true;
    internal void Jmp() => s.PC = op.Addr;
    internal void Lda() => p.AC = op.V;
    internal void Ldx() => p.X = op.V;
    internal void Ldy() => p.Y = op.V;
    internal void Nop() { }
    internal void Ora() => p.AC |= op.V;
    internal void Tax() => p.X = p.AC;
    internal void Txa() => p.AC = p.X;
    internal void Tay() => p.Y = p.AC;
    internal void Tya() => p.AC = p.Y;
    internal void Dex() => p.X--;
    internal void Inx() => p.X++;
    internal void Dey() => p.Y--;
    internal void Iny() => p.Y++;
    internal void Rts() => s.PC = (ushort)(p.PopWord() + 1);
    internal void Sbc() => p.AC = p.SubWithBorrow(op.V);
    internal void Txs() => s.SP = p.X;
    internal void Tsx() => p.X = s.SP;
    internal void Pha() => p.Push(p.AC);
    internal void Pla() => p.AC = p.Pop();

    internal void Bit()
    {
        var v = op.V;
        s.Zero = (p.AC & v) == 0;
        s.Negative = (v & 0b1000_0000) != 0;
        s.Overflow = (v & 0b0100_0000) != 0;
    }

    internal void Brk()
    {
        s.PC++;
        p.PushWord(s.PC);
        p.Push((byte)(s.SR | CpuStatus.Break | CpuStatus.Unused));
        s.Interrupt = true;
        s.PC = m.Word(0xFFFE);
    }

    internal void Jsr()
    {
        p.PushWord((ushort)(s.PC - 1));
        s.PC = op.Addr;
    }

    internal void Rti()
    {
        byte flags = p.Pop();
        s.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        s.PC = p.PopWord();
    }

    internal void Php()
    {
        byte flags = (byte)(s.SR | CpuStatus.Break | CpuStatus.Unused);
        p.Push(flags);
    }

    internal void Plp()
    {
        byte flags = p.Pop();
        s.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
    }
}
