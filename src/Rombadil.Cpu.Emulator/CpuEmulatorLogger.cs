namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorLogger(Memory<byte> memory, CpuEmulator6502 cpu)
{
    public string Log()
    {
        var reg = cpu.Reg;
        var opcode = (CpuOpcode)memory.Span[reg.PC];
        var low = memory.Span[reg.PC + 1];
        var high = memory.Span[reg.PC + 2];

        if (!CpuOpcodeMap.TryDecodeOpcode(opcode, out var decode))
            return string.Empty;

        var (instruction, mode) = decode;
        var (addr, baseAddr) = cpu.State.ResolveAddr((ushort)(reg.PC + 1), mode);

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

        string dissassemble = $"{instruction} {operand}";

        if (ShouldShowMemoryValue(instruction, mode))
        {
            var display = $"{memory.Span[addr]:X2}";
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

        long totalPpuCycles = cpu.Cycles * 3;
        long scanline = totalPpuCycles / 341;
        long dot = totalPpuCycles % 341;

        string ppuText = $"PPU:{scanline,3},{dot,3}";
        return string.Format("{0:X4}  {1, -8}  {2, -32}A:{3:X2} X:{4:X2} Y:{5:X2} P:{6:X2} SP:{7:X2} {8} CYC:{9}",
            reg.PC, directOp, dissassemble, reg.AC, reg.X, reg.Y, (byte)reg.SR, reg.SP, ppuText, cpu.Cycles);
    }

    private static bool ShouldShowMemoryValue(CpuInstruction instruction, CpuAddressingMode mode)
    {
        if (instruction is CpuInstruction.JMP or CpuInstruction.JSR or CpuInstruction.RTS or CpuInstruction.RTI or
            CpuInstruction.BEQ or CpuInstruction.BNE or CpuInstruction.BCS or CpuInstruction.BCC or
            CpuInstruction.BVS or CpuInstruction.BVC or CpuInstruction.BMI or CpuInstruction.BPL)
            return false;

        return mode is CpuAddressingMode.ZeroPage or CpuAddressingMode.ZeroPageX or CpuAddressingMode.ZeroPageY or
            CpuAddressingMode.Absolute or CpuAddressingMode.AbsoluteX or CpuAddressingMode.AbsoluteY or
            CpuAddressingMode.IndirectX or CpuAddressingMode.IndirectY;
    }

}
