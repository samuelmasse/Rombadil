namespace Rombadil.Cpu.Emulator;

internal class CpuEmulatorTimings
{
    private readonly Dictionary<(CpuInstruction, CpuAddressingMode), (byte, byte)> timings = [];

    internal (byte, byte) this[CpuInstruction op, CpuAddressingMode mode] => timings[(op, mode)];

    internal CpuEmulatorTimings()
    {
        Register(CpuInstruction.ADC,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.AND,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.ASL,
            (CpuAddressingMode.Accumulator, 2, 0),
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0)
        );

        Register(CpuInstruction.BIT,
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.Absolute, 4, 0)
        );

        Register(CpuInstruction.BPL, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BMI, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BVC, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BVS, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BCC, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BCS, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BNE, (CpuAddressingMode.Relative, 2, 0));
        Register(CpuInstruction.BEQ, (CpuAddressingMode.Relative, 2, 0));

        Register(CpuInstruction.BRK, (CpuAddressingMode.Implied, 7, 0));

        Register(CpuInstruction.CMP,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.CPX,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.Absolute, 4, 0)
        );

        Register(CpuInstruction.CPY,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.Absolute, 4, 0)
        );

        Register(CpuInstruction.DEC,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0)
        );

        Register(CpuInstruction.EOR,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.CLC, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.SEC, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.CLI, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.SEI, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.CLV, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.CLD, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.SED, (CpuAddressingMode.Implied, 2, 0));

        Register(CpuInstruction.INC,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0)
        );

        Register(CpuInstruction.JMP,
            (CpuAddressingMode.Absolute, 3, 0),
            (CpuAddressingMode.Indirect, 5, 0)
        );

        Register(CpuInstruction.JSR, (CpuAddressingMode.Absolute, 6, 0));

        Register(CpuInstruction.LDA,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.LDX,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageY, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteY, 4, 1)
        );

        Register(CpuInstruction.LDY,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1)
        );

        Register(CpuInstruction.LSR,
            (CpuAddressingMode.Accumulator, 2, 0),
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0)
        );

        Register(CpuInstruction.NOP, (CpuAddressingMode.Implied, 2, 0));

        Register(CpuInstruction.ORA,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.TAX, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.TXA, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.DEX, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.INX, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.TAY, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.TYA, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.DEY, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.INY, (CpuAddressingMode.Implied, 2, 0));

        Register(CpuInstruction.ROL,
            (CpuAddressingMode.Accumulator, 2, 0),
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0)
        );

        Register(CpuInstruction.ROR,
            (CpuAddressingMode.Accumulator, 2, 0),
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0)
        );

        Register(CpuInstruction.RTI, (CpuAddressingMode.Implied, 6, 0));

        Register(CpuInstruction.RTS, (CpuAddressingMode.Implied, 6, 0));

        Register(CpuInstruction.SBC,
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuInstruction.STA,
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 5, 0),
            (CpuAddressingMode.AbsoluteY, 5, 0),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 6, 0)
        );

        Register(CpuInstruction.TXS, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.TSX, (CpuAddressingMode.Implied, 2, 0));
        Register(CpuInstruction.PHA, (CpuAddressingMode.Implied, 3, 0));
        Register(CpuInstruction.PLA, (CpuAddressingMode.Implied, 4, 0));
        Register(CpuInstruction.PHP, (CpuAddressingMode.Implied, 3, 0));
        Register(CpuInstruction.PLP, (CpuAddressingMode.Implied, 4, 0));

        Register(CpuInstruction.STX,
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageY, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0)
        );

        Register(CpuInstruction.STY,
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0)
        );
    }

    private void Register(CpuInstruction instr, params (CpuAddressingMode, byte, byte)[] variants)
    {
        foreach (var (mode, cycles, pagePenalty) in variants)
            timings.Add((instr, mode), (cycles, pagePenalty));
    }
}
