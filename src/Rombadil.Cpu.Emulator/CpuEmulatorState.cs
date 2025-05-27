namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorState(Memory<byte> memory)
{
    private CpuEmulatorRegisters reg;
    private long cycles;

    internal Span<byte> Mem => memory.Span;
    internal ref CpuEmulatorRegisters Reg => ref reg;
    internal ref long Cycles => ref cycles;

    internal void Push(byte value) => memory.Span[0x0100 + reg.SP--] = value;
    internal byte Pop() => memory.Span[0x0100 + ++reg.SP];

    internal void PushWord(ushort value)
    {
        Push((byte)((value >> 8) & 0xFF));
        Push((byte)(value & 0xFF));
    }

    internal ushort PopWord()
    {
        byte low = Pop();
        byte high = Pop();
        return (ushort)(low | (high << 8));
    }

    internal ushort ReadWord() => ReadWord(reg.PC);
    internal ushort ReadWord(ushort pc) => (ushort)(memory.Span[pc] | (memory.Span[pc + 1] << 8));

    internal void UpdateZeroNegativeFlags(byte value)
    {
        SetFlag(CpuStatus.Zero, value == 0);
        SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
    }

    internal void SetFlag(CpuStatus flag, bool on)
    {
        if (on) reg.SR |= flag;
        else reg.SR &= ~flag;
    }

    internal void Tick(CpuInstruction instr)
    {
        cycles += CpuEmulatorTimings.Get(instr, CpuAddressingMode.Implied).Cycles;
    }

    internal ushort Addr(CpuInstruction instr, CpuAddressingMode mode)
    {
        var timing = CpuEmulatorTimings.Get(instr, mode);
        return Addr(timing, mode);
    }

    internal ushort AddrIllegal(CpuEmulatorIllegalInstruction instr, CpuAddressingMode mode)
    {
        var timing = CpuEmulatorIllegalTimings.Get(instr, mode);
        return Addr(timing, mode);
    }

    internal ushort Addr((byte Cycles, byte PagePenalty) timing, CpuAddressingMode mode)
    {
        var (addr, baseAddr) = ResolveAddr(reg.PC, mode);
        reg.PC += (ushort)CpuAddressingModeSize.Get(mode);

        cycles += timing.Cycles;
        if ((baseAddr & 0xFF00) != (addr & 0xFF00))
            cycles += timing.PagePenalty;

        return addr;
    }

    internal (ushort addr, ushort baseAddr) ResolveAddr(ushort pc, CpuAddressingMode mode)
    {
        var mem = memory.Span;

        if (mode == CpuAddressingMode.Immediate)
            return (pc, pc);
        else if (mode == CpuAddressingMode.ZeroPage)
        {
            ushort b = mem[pc];
            return (b, b);
        }
        else if (mode == CpuAddressingMode.ZeroPageX)
        {
            ushort b = mem[pc];
            ushort e = (byte)(b + reg.X);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.ZeroPageY)
        {
            ushort b = mem[pc];
            ushort e = (byte)(b + reg.Y);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.Absolute)
        {
            ushort b = ReadWord(pc);
            return (b, b);
        }
        else if (mode == CpuAddressingMode.AbsoluteX)
        {
            ushort b = ReadWord(pc);
            ushort e = (ushort)(b + reg.X);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.AbsoluteY)
        {
            ushort b = ReadWord(pc);
            ushort e = (ushort)(b + reg.Y);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.IndirectX)
        {
            ushort b = mem[pc];
            byte zpx = (byte)(b + reg.X);
            ushort e = (ushort)(mem[zpx] | (mem[(byte)(zpx + 1)] << 8));
            return (e, b);
        }
        else if (mode == CpuAddressingMode.IndirectY)
        {
            ushort b = mem[pc];
            ushort indirect = (ushort)(mem[b] | (mem[(byte)(b + 1)] << 8));
            ushort e = (ushort)(indirect + reg.Y);
            return (e, indirect);
        }
        else if (mode == CpuAddressingMode.Indirect)
        {
            ushort b = ReadWord(pc);
            ushort addr = (ushort)((b & 0xFF00) | ((b + 1) & 0x00FF));
            ushort e = (ushort)(mem[b] | (mem[addr] << 8));
            return (e, b);
        }
        else return (0, 0);
    }

    internal void Branch(CpuInstruction instr, bool condition)
    {
        var timing = CpuEmulatorTimings.Get(instr, CpuAddressingMode.Relative);
        cycles += timing.Cycles;

        sbyte offset = (sbyte)memory.Span[reg.PC++];
        if (!condition)
            return;

        cycles += 1;

        ushort originalPC = reg.PC;
        reg.PC = (ushort)(reg.PC + offset);

        if ((originalPC & 0xFF00) != (reg.PC & 0xFF00))
            cycles += timing.PagePenalty;
    }
}
