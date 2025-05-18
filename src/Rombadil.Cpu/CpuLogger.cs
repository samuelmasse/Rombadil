namespace Rombadil.Cpu;

public class CpuLogger(Memory<byte> memory, Cpu6502 cpu)
{
    public void Step()
    {
        var reg = cpu.Reg;
        var opcode = (CpuOpcode)memory.Span[reg.PC];

        int[] direct = [(byte)opcode, -1, -1];
        string dissassemble = "";

        if (opcode == CpuOpcode.LDA_IM)
        {
            direct[1] = memory.Span[reg.PC + 1];
            dissassemble = $"LDA #${direct[1]:X2}";
        }
        else if (opcode == CpuOpcode.STA_ABS)
        {
            direct[1] = memory.Span[reg.PC + 1];
            direct[2] = memory.Span[reg.PC + 2];
            dissassemble = $"STA ${(ushort)(direct[1] | direct[2] << 8):X4}";
        }
        else if (opcode == CpuOpcode.TAX)
            dissassemble = "TAX";
        else if (opcode == CpuOpcode.INX)
            dissassemble = "INX";
        else if (opcode == CpuOpcode.DEX)
            dissassemble = "DEX";

        string directOp = $"{direct[0]:X2}";
        if (direct[1] >= 0)
        {
            directOp += $" {direct[1]:X2}";

            if (direct[2] >= 0)
                directOp += $" {direct[2]:X2}";
        }

        Console.WriteLine(string.Format("{0:X4}  {1, -8}  {2, -32}A:{3:X2} X:{4:X2} Y:{5:X2} P:{6:X2} SP:{7:X2} CYC:{8}",
            reg.PC, directOp, dissassemble, reg.AC, reg.X, reg.Y, (byte)reg.SR, reg.SP, cpu.Cycles));

        cpu.Step();
    }
}
