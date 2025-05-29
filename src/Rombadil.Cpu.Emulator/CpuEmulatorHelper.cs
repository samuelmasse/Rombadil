namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorHelper(CpuEmulatorState state, CpuEmulatorMemory memory)
{
    internal ref ushort PC => ref state.Reg.PC;
    internal ref CpuStatus SR => ref state.Reg.SR;
    internal ref byte SP => ref state.Reg.SP;

    internal void Push(byte value) => memory[(ushort)(0x0100 + state.Reg.SP--)] = value;
    internal byte Pop() => memory[(ushort)(0x0100 + ++state.Reg.SP)];
    internal ushort ReadWord(ushort pc) => (ushort)(memory[pc] | (memory[(ushort)(pc + 1)] << 8));

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

    internal ushort Addr((byte Cycles, byte PagePenalty) timing, CpuAddressingMode mode)
    {
        var (addr, baseAddr) = ResolveAddr(state.Reg.PC, mode);
        state.Reg.PC += (ushort)CpuAddressingModeSize.Get(mode);

        state.Cycles += timing.Cycles;
        if ((baseAddr & 0xFF00) != (addr & 0xFF00))
            state.Cycles += timing.PagePenalty;

        return addr;
    }

    internal void Branch(bool condition)
    {
        sbyte offset = (sbyte)memory[(ushort)(state.Reg.PC - 1)];
        if (!condition)
            return;

        state.Cycles++;

        ushort originalPC = state.Reg.PC;
        state.Reg.PC = (ushort)(state.Reg.PC + offset);

        if ((originalPC & 0xFF00) != (state.Reg.PC & 0xFF00))
            state.Cycles++;
    }

    internal (ushort addr, ushort baseAddr) ResolveAddr(ushort pc, CpuAddressingMode mode)
    {
        if (mode == CpuAddressingMode.Immediate)
            return (pc, pc);
        else if (mode == CpuAddressingMode.ZeroPage)
        {
            ushort b = memory[pc];
            return (b, b);
        }
        else if (mode == CpuAddressingMode.ZeroPageX)
        {
            ushort b = memory[pc];
            ushort e = (byte)(b + state.Reg.X);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.ZeroPageY)
        {
            ushort b = memory[pc];
            ushort e = (byte)(b + state.Reg.Y);
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
            ushort e = (ushort)(b + state.Reg.X);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.AbsoluteY)
        {
            ushort b = ReadWord(pc);
            ushort e = (ushort)(b + state.Reg.Y);
            return (e, b);
        }
        else if (mode == CpuAddressingMode.IndirectX)
        {
            ushort b = memory[pc];
            byte zpx = (byte)(b + state.Reg.X);
            ushort e = (ushort)(memory[zpx] | (memory[(byte)(zpx + 1)] << 8));
            return (e, b);
        }
        else if (mode == CpuAddressingMode.IndirectY)
        {
            ushort b = memory[pc];
            ushort indirect = (ushort)(memory[b] | (memory[(byte)(b + 1)] << 8));
            ushort e = (ushort)(indirect + state.Reg.Y);
            return (e, indirect);
        }
        else if (mode == CpuAddressingMode.Indirect)
        {
            ushort b = ReadWord(pc);
            ushort addr = (ushort)((b & 0xFF00) | ((b + 1) & 0x00FF));
            ushort e = (ushort)(memory[b] | (memory[addr] << 8));
            return (e, b);
        }
        else return (0, 0);
    }
}
