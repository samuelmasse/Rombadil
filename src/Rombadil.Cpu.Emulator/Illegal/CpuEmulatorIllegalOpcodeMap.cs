namespace Rombadil.Cpu.Emulator;

public static class CpuEmulatorIllegalOpcodeMap
{
    private static readonly (CpuEmulatorIllegalInstruction, CpuAddressingMode)?[] fromOpcode =
        new (CpuEmulatorIllegalInstruction, CpuAddressingMode)?[0x100];

    static CpuEmulatorIllegalOpcodeMap()
    {
        Register(CpuEmulatorIllegalInstruction.NOP,
            (CpuAddressingMode.Implied, CpuEmulatorIllegalOpcode.NOP_1A),
            (CpuAddressingMode.Implied, CpuEmulatorIllegalOpcode.NOP_3A),
            (CpuAddressingMode.Implied, CpuEmulatorIllegalOpcode.NOP_5A),
            (CpuAddressingMode.Implied, CpuEmulatorIllegalOpcode.NOP_7A),
            (CpuAddressingMode.Implied, CpuEmulatorIllegalOpcode.NOP_DA),
            (CpuAddressingMode.Implied, CpuEmulatorIllegalOpcode.NOP_FA),

            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.NOP_IM_80),
            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.NOP_IM_82),
            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.NOP_IM_89),
            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.NOP_IM_C2),
            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.NOP_IM_E2),

            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.NOP_ZP_04),
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.NOP_ZP_44),
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.NOP_ZP_64),

            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.NOP_ZPX_14),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.NOP_ZPX_34),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.NOP_ZPX_54),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.NOP_ZPX_74),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.NOP_ZPX_D4),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.NOP_ZPX_F4),

            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.NOP_ABS_0C),

            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.NOP_ABSX_1C),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.NOP_ABSX_3C),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.NOP_ABSX_5C),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.NOP_ABSX_7C),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.NOP_ABSX_DC),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.NOP_ABSX_FC)
        );

        Register(CpuEmulatorIllegalInstruction.LAX,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.LAX_ZP),
            (CpuAddressingMode.ZeroPageY, CpuEmulatorIllegalOpcode.LAX_ZPY),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.LAX_ABS),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.LAX_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.LAX_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.LAX_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.SAX,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.SAX_ZP),
            (CpuAddressingMode.ZeroPageY, CpuEmulatorIllegalOpcode.SAX_ZPY),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.SAX_ABS),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.SAX_INDX)
        );

        Register(CpuEmulatorIllegalInstruction.SBC, (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.SBC_IM_EB));

        Register(CpuEmulatorIllegalInstruction.DCP,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.DCP_ZP),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.DCP_ZPX),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.DCP_ABS),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.DCP_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.DCP_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.DCP_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.DCP_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.ISB,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.ISB_ZP),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.ISB_ZPX),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.ISB_ABS),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.ISB_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.ISB_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.ISB_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.ISB_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.SLO,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.SLO_ZP),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.SLO_ZPX),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.SLO_ABS),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.SLO_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.SLO_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.SLO_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.SLO_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.RLA,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.RLA_ZP),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.RLA_ZPX),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.RLA_ABS),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.RLA_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.RLA_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.RLA_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.RLA_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.SRE,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.SRE_ZP),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.SRE_ZPX),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.SRE_ABS),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.SRE_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.SRE_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.SRE_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.SRE_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.RRA,
            (CpuAddressingMode.ZeroPage, CpuEmulatorIllegalOpcode.RRA_ZP),
            (CpuAddressingMode.ZeroPageX, CpuEmulatorIllegalOpcode.RRA_ZPX),
            (CpuAddressingMode.Absolute, CpuEmulatorIllegalOpcode.RRA_ABS),
            (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.RRA_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.RRA_ABSY),
            (CpuAddressingMode.IndirectX, CpuEmulatorIllegalOpcode.RRA_INDX),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.RRA_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.AAC,
            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.AAC_IM_0B),
            (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.AAC_IM_2B)
        );

        Register(CpuEmulatorIllegalInstruction.ASR, (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.ASR_IM));
        Register(CpuEmulatorIllegalInstruction.ARR, (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.ARR_IM));
        Register(CpuEmulatorIllegalInstruction.ATX, (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.ATX_IM));
        Register(CpuEmulatorIllegalInstruction.AXS, (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.AXS_IM));
        Register(CpuEmulatorIllegalInstruction.XAA, (CpuAddressingMode.Immediate, CpuEmulatorIllegalOpcode.XAA_IM));

        Register(CpuEmulatorIllegalInstruction.LAR, (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.LAR_ABSY));

        Register(CpuEmulatorIllegalInstruction.AXA,
            (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.AXA_ABSY),
            (CpuAddressingMode.IndirectY, CpuEmulatorIllegalOpcode.AXA_INDY)
        );

        Register(CpuEmulatorIllegalInstruction.SYA, (CpuAddressingMode.AbsoluteX, CpuEmulatorIllegalOpcode.SYA_ABSX));
        Register(CpuEmulatorIllegalInstruction.SXA, (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.SXA_ABSY));
        Register(CpuEmulatorIllegalInstruction.XAS, (CpuAddressingMode.AbsoluteY, CpuEmulatorIllegalOpcode.XAS_ABSY));
    }

    public static bool TryDecodeOpcode(CpuEmulatorIllegalOpcode opcode, out (CpuEmulatorIllegalInstruction, CpuAddressingMode) decode)
    {
        var val = fromOpcode[(byte)opcode];
        decode = val.GetValueOrDefault();
        return val != null;
    }

    private static void Register(CpuEmulatorIllegalInstruction instr, params (CpuAddressingMode, CpuEmulatorIllegalOpcode)[] variants)
    {
        foreach (var (mode, opcode) in variants)
            fromOpcode[(byte)opcode] = (instr, mode);
    }
}
