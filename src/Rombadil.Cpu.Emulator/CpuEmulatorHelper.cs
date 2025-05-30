namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorHelper(CpuEmulatorState state, CpuEmulatorMemory memory)
{
    internal void Push(byte value) => memory[(ushort)(0x0100 + state.SP--)] = value;
    internal byte Pop() => memory[(ushort)(0x0100 + ++state.SP)];

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

    internal void Branch(bool condition)
    {
        sbyte offset = (sbyte)memory[(ushort)(state.PC - 1)];
        if (!condition)
            return;

        state.Cycles++;

        ushort originalPC = state.PC;
        state.PC = (ushort)(state.PC + offset);

        if ((originalPC & 0xFF00) != (state.PC & 0xFF00))
            state.Cycles++;
    }

    internal (ushort, ushort) Resolve(ushort pc, CpuAddressingMode mode)
    {
        return mode switch
        {
            CpuAddressingMode.Immediate => (pc, pc),
            CpuAddressingMode.ZeroPage => (memory[pc], memory[pc]),
            CpuAddressingMode.ZeroPageX => ((byte)(memory[pc] + state.X), memory[pc]),
            CpuAddressingMode.ZeroPageY => ((byte)(memory[pc] + state.Y), memory[pc]),
            CpuAddressingMode.Absolute => (memory.Word(pc), memory.Word(pc)),
            CpuAddressingMode.AbsoluteX => ((ushort)(memory.Word(pc) + state.X), memory.Word(pc)),
            CpuAddressingMode.AbsoluteY => ((ushort)(memory.Word(pc) + state.Y), memory.Word(pc)),
            CpuAddressingMode.Indirect => (memory.WordPageWrap(memory.Word(pc)), memory.Word(pc)),
            CpuAddressingMode.IndirectX => (memory.WordZP((byte)(memory[pc] + state.X)), memory[pc]),
            CpuAddressingMode.IndirectY => ((ushort)(memory.WordZP(memory[pc]) + state.Y), memory.WordZP(memory[pc])),
            _ => (0, 0)
        };
    }
}
