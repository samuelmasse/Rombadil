namespace Rombadil.Cpu.Emulator;

public class CpuLogger(Memory<byte> memory, Cpu6502 cpu)
{
    public void Log()
    {
        var reg = cpu.Reg;
        var opcode = (CpuOpcode)memory.Span[reg.PC];
        if (!CpuOpcodeMap.TryDecodeOpcode(opcode, out var decode))
            return;

        var (instruction, addressingMode) = decode;
        string operand = string.Empty;

        int size = CpuAddressingModeSize.Get(addressingMode);
        int[] direct = [(byte)opcode, -1, -1];

        if (size == 1)
        {
            direct[1] = memory.Span[reg.PC + 1];
            operand = $"{direct[1]:X2}";
        }
        else if (size == 2)
        {
            direct[1] = memory.Span[reg.PC + 1];
            direct[2] = memory.Span[reg.PC + 2];
            operand = $"{direct[1] | direct[2] << 8:X4}";
        }

        string dissassemble = $"{instruction} {FormatArgument(addressingMode, operand)}".Trim();

        string directOp = $"{direct[0]:X2}";
        if (direct[1] >= 0)
        {
            directOp += $" {direct[1]:X2}";

            if (direct[2] >= 0)
                directOp += $" {direct[2]:X2}";
        }

        Console.WriteLine(string.Format("{0:X4}  {1, -8}  {2, -32}A:{3:X2} X:{4:X2} Y:{5:X2} P:{6:X2} SP:{7:X2} CYC:{8}",
            reg.PC, directOp, dissassemble, reg.AC, reg.X, reg.Y, (byte)reg.SR, reg.SP, cpu.Cycles));
    }

    private static string FormatArgument(CpuAddressingMode mode, string operand)
    {
        return mode switch
        {
            CpuAddressingMode.Immediate => $"#{operand}",
            CpuAddressingMode.Relative => operand,
            CpuAddressingMode.Indirect => $"({operand})",
            CpuAddressingMode.ZeroPage => operand,
            CpuAddressingMode.ZeroPageX => $"{operand},X",
            CpuAddressingMode.ZeroPageY => $"{operand},Y",
            CpuAddressingMode.Absolute => operand,
            CpuAddressingMode.AbsoluteX => $"{operand},X",
            CpuAddressingMode.AbsoluteY => $"{operand},Y",
            CpuAddressingMode.IndirectX => $"({operand},X)",
            CpuAddressingMode.IndirectY => $"({operand}),Y",
            _ => string.Empty
        };
    }
}
