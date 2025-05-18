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
            case CpuOpcode.TAX:
                Tax();
                break;
            case CpuOpcode.INX:
                Inx();
                break;
            case CpuOpcode.DEX:
                Dex();
                break;
            case CpuOpcode.BEQ:
                Beq();
                break;
            case CpuOpcode.CLC:
                Clc();
                break;
            case CpuOpcode.ADC_IM:
                AdcIm();
                break;
            case CpuOpcode.JSR:
                Jsr();
                break;
            case CpuOpcode.RTS:
                Rts();
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

    private void Tax()
    {
        reg.X = reg.AC;
        UpdateZeroNegativeFlags(reg.X);
        cycles += 2;
    }

    private void Inx()
    {
        reg.X++;
        UpdateZeroNegativeFlags(reg.X);
        cycles += 2;
    }

    private void Dex()
    {
        reg.X--;
        UpdateZeroNegativeFlags(reg.X);
        cycles += 2;
    }

    private void Beq()
    {
        sbyte offset = (sbyte)memory.Span[reg.PC++];
        if ((reg.SR & CpuStatus.Zero) != 0)
        {
            ushort oldPC = reg.PC;
            reg.PC = (ushort)(reg.PC + offset);
            cycles += 1;
            if ((oldPC & 0xFF00) != (reg.PC & 0xFF00))
                cycles += 1;
        }
        cycles += 2;
    }

    private void Clc()
    {
        SetFlag(CpuStatus.Carry, false);
        cycles += 2;
    }

    private void AdcIm()
    {
        byte value = memory.Span[reg.PC++];
        int carry = (reg.SR & CpuStatus.Carry) != 0 ? 1 : 0;
        int result = reg.AC + value + carry;

        SetFlag(CpuStatus.Carry, result > 0xFF);
        byte resultByte = (byte)result;

        SetFlag(CpuStatus.Overflow, ((reg.AC ^ resultByte) & (value ^ resultByte) & 0x80) != 0);

        reg.AC = resultByte;
        UpdateZeroNegativeFlags(reg.AC);
        cycles += 2;
    }

    private void Jsr()
    {
        ushort addr = (ushort)(memory.Span[reg.PC] | (memory.Span[reg.PC + 1] << 8));
        ushort returnAddr = (ushort)(reg.PC + 1);

        Push((byte)((returnAddr >> 8) & 0xFF));
        Push((byte)(returnAddr & 0xFF));

        reg.PC = addr;
        cycles += 6;
    }

    private void Rts()
    {
        byte low = Pop();
        byte high = Pop();
        reg.PC = (ushort)(((high << 8) | low) + 1);
        cycles += 6;
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

    private void Push(byte value)
    {
        memory.Span[0x0100 + reg.SP--] = value;
    }

    private byte Pop()
    {
        return memory.Span[0x0100 + ++reg.SP];
    }
}

