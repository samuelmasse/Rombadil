namespace Rombadil.Cpu;

public class Cpu6502(Memory<byte> memory)
{
    private CpuRegisters reg;
    private long cycles;

    public CpuRegisters Reg => reg;
    public long Cycles => cycles;

    public void Reset()
    {
        reg.PC = (ushort)(memory.Span[0xFFFC] | (memory.Span[0xFFFD] << 8));

        reg.AC = 0;
        reg.X = 0;
        reg.Y = 0;

        reg.SR = CpuStatus.InterruptDisable | CpuStatus.Unused;
        reg.SP = 0xFD;

        cycles = 7;
    }

    public void Step()
    {
        var opcode = (CpuOpcode)memory.Span[reg.PC++];
        Execute(opcode);
    }

    private void Execute(CpuOpcode opcode)
    {
        switch (opcode)
        {
            case CpuOpcode.LDA_IM:
                LdaIm();
                break;
            case CpuOpcode.STA_ABS:
                StaAbs();
                break;
            default:
                break;
        }
    }

    private void LdaIm()
    {
        byte value = memory.Span[reg.PC++];
        reg.AC = value;
        UpdateZeroNegativeFlags(value);
        cycles += 2;
    }

    private void StaAbs()
    {
        ushort addr = (ushort)(memory.Span[reg.PC++] | (memory.Span[reg.PC++] << 8));
        memory.Span[addr] = reg.AC;
        cycles += 4;
    }

    private void UpdateZeroNegativeFlags(byte value)
    {
        SetFlag(CpuStatus.Zero, value == 0);
        SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
    }

    private void SetFlag(CpuStatus flag, bool on)
    {
        if (on) reg.SR |= flag;
        else reg.SR &= ~flag;
    }
}

