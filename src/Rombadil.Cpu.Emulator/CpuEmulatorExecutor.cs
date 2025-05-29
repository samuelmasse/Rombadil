namespace Rombadil.Cpu.Emulator;

internal ref struct CpuEmulatorExecutor(CpuEmulatorState cpu, ushort addr, ref byte cvalue)
{
    private ref byte value = ref cvalue;

    internal void Asl() => value = cpu.ShiftLeft(value);
    internal void Lsr() => value = cpu.ShiftRight(value);
    internal void Rol() => value = cpu.RotateLeft(value);
    internal void Ror() => value = cpu.RotateRight(value);
    internal void Sta() => value = cpu.AC;
    internal void Stx() => value = cpu.X;
    internal void Sty() => value = cpu.Y;

    internal void Dec()
    {
        value--;
        cpu.SetZN(value);
    }

    internal void Inc()
    {
        value++;
        cpu.SetZN(value);
    }

    internal readonly void Adc() => cpu.AC = cpu.AddWithCarry(value);
    internal readonly void And() => cpu.AC &= value;
    internal readonly void Bpl() => cpu.Branch(!cpu.HasFlag(CpuStatus.Negative));
    internal readonly void Bmi() => cpu.Branch(cpu.HasFlag(CpuStatus.Negative));
    internal readonly void Bvc() => cpu.Branch(!cpu.HasFlag(CpuStatus.Overflow));
    internal readonly void Bvs() => cpu.Branch(cpu.HasFlag(CpuStatus.Overflow));
    internal readonly void Bcc() => cpu.Branch(!cpu.HasFlag(CpuStatus.Carry));
    internal readonly void Bcs() => cpu.Branch(cpu.HasFlag(CpuStatus.Carry));
    internal readonly void Bne() => cpu.Branch(!cpu.HasFlag(CpuStatus.Zero));
    internal readonly void Beq() => cpu.Branch(cpu.HasFlag(CpuStatus.Zero));
    internal readonly void Cmp() => cpu.Compare(cpu.AC, value);
    internal readonly void Cpx() => cpu.Compare(cpu.X, value);
    internal readonly void Cpy() => cpu.Compare(cpu.Y, value);
    internal readonly void Eor() => cpu.AC ^= value;
    internal readonly void Clc() => cpu.SetFlag(CpuStatus.Carry, false);
    internal readonly void Sec() => cpu.SetFlag(CpuStatus.Carry, true);
    internal readonly void Cli() => cpu.SetFlag(CpuStatus.Interrupt, false);
    internal readonly void Sei() => cpu.SetFlag(CpuStatus.Interrupt, true);
    internal readonly void Clv() => cpu.SetFlag(CpuStatus.Overflow, false);
    internal readonly void Cld() => cpu.SetFlag(CpuStatus.Decimal, false);
    internal readonly void Sed() => cpu.SetFlag(CpuStatus.Decimal, true);
    internal readonly void Jmp() => cpu.PC = addr;
    internal readonly void Lda() => cpu.AC = value;
    internal readonly void Ldx() => cpu.X = value;
    internal readonly void Ldy() => cpu.Y = value;
    internal readonly void Nop() { }
    internal readonly void Ora() => cpu.AC |= value;
    internal readonly void Tax() => cpu.X = cpu.AC;
    internal readonly void Txa() => cpu.AC = cpu.X;
    internal readonly void Tay() => cpu.Y = cpu.AC;
    internal readonly void Tya() => cpu.AC = cpu.Y;
    internal readonly void Dex() => cpu.X--;
    internal readonly void Inx() => cpu.X++;
    internal readonly void Dey() => cpu.Y--;
    internal readonly void Iny() => cpu.Y++;
    internal readonly void Rts() => cpu.PC = (ushort)(cpu.PopWord() + 1);
    internal readonly void Sbc() => cpu.AC = cpu.SubWithBorrow(value);
    internal readonly void Txs() => cpu.SP = cpu.X;
    internal readonly void Tsx() => cpu.X = cpu.SP;
    internal readonly void Pha() => cpu.Push(cpu.AC);
    internal readonly void Pla() => cpu.AC = cpu.Pop();

    internal readonly void Bit()
    {
        cpu.SetFlag(CpuStatus.Zero, (cpu.AC & value) == 0);
        cpu.SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
        cpu.SetFlag(CpuStatus.Overflow, (value & 0b0100_0000) != 0);
    }

    internal readonly void Brk()
    {
        cpu.PC++;
        cpu.PushWord(cpu.PC);
        cpu.Push((byte)(cpu.SR | CpuStatus.Break | CpuStatus.Unused));
        cpu.SetFlag(CpuStatus.Interrupt, true);
        cpu.PC = cpu.ReadWord(0xFFFE);
    }

    internal readonly void Jsr()
    {
        cpu.PushWord((ushort)(cpu.PC - 1));
        cpu.PC = addr;
    }

    internal readonly void Rti()
    {
        byte flags = cpu.Pop();
        cpu.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
        cpu.PC = cpu.PopWord();
    }

    internal readonly void Php()
    {
        byte flags = (byte)(cpu.SR | CpuStatus.Break | CpuStatus.Unused);
        cpu.Push(flags);
    }

    internal readonly void Plp()
    {
        byte flags = cpu.Pop();
        cpu.SR = (CpuStatus)((flags & ~(byte)CpuStatus.Break) | (byte)CpuStatus.Unused);
    }
}
