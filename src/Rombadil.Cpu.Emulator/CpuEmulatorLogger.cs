namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorLogger(CpuEmulatorState state, CpuEmulatorMemory memory, CpuEmulator6502 cpu)
{
    public string Log()
    {
        var reg = state.Reg;
        var opcode = (CpuOpcode)memory[reg.PC];
        var low = memory[(ushort)(reg.PC + 1)];
        var high = memory[(ushort)(reg.PC + 2)];

        CpuInstruction? instruction = null;
        CpuEmulatorIllegalInstruction? illegalInstruction = null;
        CpuAddressingMode mode;

        if (CpuOpcodeMap.TryDecodeOpcode((CpuOpcode)memory[reg.PC], out var decode))
            (instruction, mode) = decode;
        else if (CpuEmulatorIllegalOpcodeMap.TryDecodeOpcode((CpuEmulatorIllegalOpcode)memory[reg.PC], out var illegal))
            (illegalInstruction, mode) = illegal;
        else return string.Empty;

        var (addr, baseAddr) = cpu.Addr((ushort)(reg.PC + 1), mode);

        string operand = string.Empty;

        if (mode == CpuAddressingMode.IndirectX)
        {
            byte zpBase = low;
            byte zpAddr = (byte)(zpBase + reg.X);
            operand = $"(${low:X2},X) @ {zpAddr:X2} = {addr:X4}";
        }
        else if (mode == CpuAddressingMode.IndirectY)
            operand = $"(${low:X2}),Y = {baseAddr:X4} @ {addr:X4}";
        else if (mode == CpuAddressingMode.Indirect)
            operand = $"(${baseAddr:X4}) = {addr:X4}";
        else if (mode == CpuAddressingMode.AbsoluteX || mode == CpuAddressingMode.AbsoluteY)
            operand = $"${baseAddr:X4},{(mode == CpuAddressingMode.AbsoluteX ? "X" : "Y")} @ {addr:X4}";
        else if (mode == CpuAddressingMode.ZeroPageX || mode == CpuAddressingMode.ZeroPageY)
            operand = $"${baseAddr:X2},{(mode == CpuAddressingMode.ZeroPageX ? "X" : "Y")} @ {addr:X2}";
        else if (mode == CpuAddressingMode.Relative)
        {
            sbyte offset = (sbyte)low;
            ushort target = (ushort)(reg.PC + 2 + offset);
            operand = $"${target:X4}";
        }
        else if (mode == CpuAddressingMode.Accumulator)
            operand = $"A";
        else if (mode == CpuAddressingMode.ZeroPage)
            operand = $"${low:X2}";
        else if (mode == CpuAddressingMode.Absolute)
            operand = $"${low | high << 8:X4}";
        else if (mode == CpuAddressingMode.Immediate)
            operand = $"#${low:X2}";

        string instr = illegalInstruction == null ? $" {instruction!.Value}" : $"*{illegalInstruction.Value}";
        string dissassemble = $"{instr} {operand}";

        if ((instruction == null || !ShouldShowMemoryValue(instruction.Value)) && ShouldShowMemoryValue(mode))
        {
            var display = $"{memory[addr]:X2}";
            dissassemble += $" = {display}";
        }

        int size = CpuAddressingModeSize.Get(mode);
        string directOp = $"{(byte)opcode:X2}";
        if (size >= 1)
        {
            directOp += $" {low:X2}";
            if (size >= 2)
                directOp += $" {high:X2}";
        }

        long totalPpuCycles = state.Cycles * 3;
        long scanline = totalPpuCycles / 341;
        long dot = totalPpuCycles % 341;

        string ppuText = $"PPU:{scanline,3},{dot,3}";
        return string.Format("{0:X4}  {1, -8} {2, -33}A:{3:X2} X:{4:X2} Y:{5:X2} P:{6:X2} SP:{7:X2} {8} CYC:{9}",
            reg.PC, directOp, dissassemble, reg.AC, reg.X, reg.Y, (byte)reg.SR, reg.SP, ppuText, state.Cycles);
    }

    private static bool ShouldShowMemoryValue(CpuInstruction instruction)
    {
        return instruction is CpuInstruction.JMP or CpuInstruction.JSR or CpuInstruction.RTS or CpuInstruction.RTI or
            CpuInstruction.BEQ or CpuInstruction.BNE or CpuInstruction.BCS or CpuInstruction.BCC or
            CpuInstruction.BVS or CpuInstruction.BVC or CpuInstruction.BMI or CpuInstruction.BPL;
    }

    private static bool ShouldShowMemoryValue(CpuAddressingMode mode)
    {
        return mode is CpuAddressingMode.ZeroPage or CpuAddressingMode.ZeroPageX or CpuAddressingMode.ZeroPageY or
            CpuAddressingMode.Absolute or CpuAddressingMode.AbsoluteX or CpuAddressingMode.AbsoluteY or
            CpuAddressingMode.IndirectX or CpuAddressingMode.IndirectY;
    }
}
