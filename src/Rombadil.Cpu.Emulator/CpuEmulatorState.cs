namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorState(Memory<byte> memory)
{
    private CpuEmulatorRegisters reg;
    private long cycles;

    public Span<byte> Mem => memory.Span;
    public ref CpuEmulatorRegisters Reg => ref reg;
    public ref long Cycles => ref cycles;

    public void Push(byte value) => memory.Span[0x0100 + reg.SP--] = value;
    public byte Pop() => memory.Span[0x0100 + ++reg.SP];

    public void UpdateZeroNegativeFlags(byte value)
    {
        SetFlag(CpuStatus.Zero, value == 0);
        SetFlag(CpuStatus.Negative, (value & 0b1000_0000) != 0);
    }

    public void SetFlag(CpuStatus flag, bool on)
    {
        if (on) reg.SR |= flag;
        else reg.SR &= ~flag;
    }

    public ushort ReadWord()
    {
        ushort value = (ushort)(memory.Span[reg.PC] | (memory.Span[reg.PC + 1] << 8));
        reg.PC += 2;
        return value;
    }

    public ushort ResolveOperandAddressWithCycles(CpuInstruction instr, CpuAddressingMode mode)
    {
        var mem = memory.Span;

        ushort? baseAddr = null;
        ushort? effectiveAddr = null;
        ushort resolved = 0;

        switch (mode)
        {
            case CpuAddressingMode.Immediate:
                resolved = reg.PC++;
                break;

            case CpuAddressingMode.ZeroPage:
                resolved = mem[reg.PC++];
                break;

            case CpuAddressingMode.ZeroPageX:
                resolved = (byte)(mem[reg.PC++] + reg.X);
                break;

            case CpuAddressingMode.ZeroPageY:
                resolved = (byte)(mem[reg.PC++] + reg.Y);
                break;

            case CpuAddressingMode.Absolute:
                resolved = ReadWord();
                break;

            case CpuAddressingMode.AbsoluteX:
                baseAddr = ReadWord();
                effectiveAddr = (ushort)(baseAddr.Value + reg.X);
                resolved = effectiveAddr.Value;
                break;

            case CpuAddressingMode.AbsoluteY:
                baseAddr = ReadWord();
                effectiveAddr = (ushort)(baseAddr.Value + reg.Y);
                resolved = effectiveAddr.Value;
                break;

            case CpuAddressingMode.IndirectX:
                byte zpx = (byte)(mem[reg.PC++] + reg.X);
                resolved = (ushort)(mem[zpx] | (mem[(byte)(zpx + 1)] << 8));
                break;

            case CpuAddressingMode.IndirectY:
                byte zpy = mem[reg.PC++];
                baseAddr = (ushort)(mem[zpy] | (mem[(byte)(zpy + 1)] << 8));
                effectiveAddr = (ushort)(baseAddr.Value + reg.Y);
                resolved = effectiveAddr.Value;
                break;
        }

        var timing = CpuEmulatorTimings.Get(instr, mode);

        cycles += timing.Cycles;
        if (baseAddr.HasValue && effectiveAddr.HasValue && (baseAddr.Value & 0xFF00) != (effectiveAddr.Value & 0xFF00))
            cycles += timing.PagePenalty;

        return resolved;
    }
}
