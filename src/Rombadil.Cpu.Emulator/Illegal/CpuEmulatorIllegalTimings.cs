namespace Rombadil.Cpu.Emulator;

public class CpuEmulatorIllegalTimings
{
    private static readonly Dictionary<(CpuEmulatorIllegalInstruction, CpuAddressingMode), (byte Cycles, byte PagePenalty)> timings = [];

    static CpuEmulatorIllegalTimings()
    {
        Register(CpuEmulatorIllegalInstruction.NOP,
            (CpuAddressingMode.Implied, 2, 0),
            (CpuAddressingMode.Immediate, 2, 0),
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageX, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteX, 4, 1)
        );

        Register(CpuEmulatorIllegalInstruction.LAX,
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageY, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.AbsoluteY, 4, 1),
            (CpuAddressingMode.IndirectX, 6, 0),
            (CpuAddressingMode.IndirectY, 5, 1)
        );

        Register(CpuEmulatorIllegalInstruction.SAX,
            (CpuAddressingMode.ZeroPage, 3, 0),
            (CpuAddressingMode.ZeroPageY, 4, 0),
            (CpuAddressingMode.Absolute, 4, 0),
            (CpuAddressingMode.IndirectX, 6, 0)
        );

        Register(CpuEmulatorIllegalInstruction.SBC, (CpuAddressingMode.Immediate, 2, 0));

        Register(CpuEmulatorIllegalInstruction.DCP,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0),
            (CpuAddressingMode.AbsoluteY, 7, 0),
            (CpuAddressingMode.IndirectX, 8, 0),
            (CpuAddressingMode.IndirectY, 8, 0)
        );

        Register(CpuEmulatorIllegalInstruction.ISB,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0),
            (CpuAddressingMode.AbsoluteY, 7, 0),
            (CpuAddressingMode.IndirectX, 8, 0),
            (CpuAddressingMode.IndirectY, 8, 0)
        );

        Register(CpuEmulatorIllegalInstruction.SLO,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0),
            (CpuAddressingMode.AbsoluteY, 7, 0),
            (CpuAddressingMode.IndirectX, 8, 0),
            (CpuAddressingMode.IndirectY, 8, 0)
        );

        Register(CpuEmulatorIllegalInstruction.RLA,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0),
            (CpuAddressingMode.AbsoluteY, 7, 0),
            (CpuAddressingMode.IndirectX, 8, 0),
            (CpuAddressingMode.IndirectY, 8, 0)
        );

        Register(CpuEmulatorIllegalInstruction.SRE,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0),
            (CpuAddressingMode.AbsoluteY, 7, 0),
            (CpuAddressingMode.IndirectX, 8, 0),
            (CpuAddressingMode.IndirectY, 8, 0)
        );

        Register(CpuEmulatorIllegalInstruction.RRA,
            (CpuAddressingMode.ZeroPage, 5, 0),
            (CpuAddressingMode.ZeroPageX, 6, 0),
            (CpuAddressingMode.Absolute, 6, 0),
            (CpuAddressingMode.AbsoluteX, 7, 0),
            (CpuAddressingMode.AbsoluteY, 7, 0),
            (CpuAddressingMode.IndirectX, 8, 0),
            (CpuAddressingMode.IndirectY, 8, 0)
        );
    }

    internal static (byte Cycles, byte PagePenalty) Get(CpuEmulatorIllegalInstruction op, CpuAddressingMode mode) =>
        timings[(op, mode)];

    private static void Register(CpuEmulatorIllegalInstruction instr,
        params (CpuAddressingMode mode, byte cycles, byte pagePenalty)[] variants)
    {
        foreach (var (mode, cycles, pagePenalty) in variants)
            timings[(instr, mode)] = (cycles, pagePenalty);
    }
}
