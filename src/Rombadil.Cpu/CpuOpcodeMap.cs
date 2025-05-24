namespace Rombadil.Cpu;

public static class CpuOpcodeMap
{
    private static readonly Dictionary<(CpuInstruction, CpuAddressingMode), CpuOpcode> toOpcode = [];
    private static readonly Dictionary<CpuOpcode, (CpuInstruction, CpuAddressingMode)> fromOpcode = [];

    static CpuOpcodeMap()
    {
        Register(CpuInstruction.ADC,
            (CpuAddressingMode.Immediate, CpuOpcode.ADC_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.ADC_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.ADC_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.ADC_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.ADC_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.ADC_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.ADC_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.ADC_INDY)
        );

        Register(CpuInstruction.AND,
            (CpuAddressingMode.Immediate, CpuOpcode.AND_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.AND_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.AND_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.AND_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.AND_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.AND_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.AND_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.AND_INDY)
        );

        Register(CpuInstruction.ASL,
            (CpuAddressingMode.Implied, CpuOpcode.ASL_ACC),
            (CpuAddressingMode.ZeroPage, CpuOpcode.ASL_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.ASL_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.ASL_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.ASL_ABSX)
        );

        Register(CpuInstruction.BIT,
            (CpuAddressingMode.ZeroPage, CpuOpcode.BIT_ZP),
            (CpuAddressingMode.Absolute, CpuOpcode.BIT_ABS)
        );

        Register(CpuInstruction.BPL, (CpuAddressingMode.Relative, CpuOpcode.BPL));
        Register(CpuInstruction.BMI, (CpuAddressingMode.Relative, CpuOpcode.BMI));
        Register(CpuInstruction.BVC, (CpuAddressingMode.Relative, CpuOpcode.BVC));
        Register(CpuInstruction.BVS, (CpuAddressingMode.Relative, CpuOpcode.BVS));
        Register(CpuInstruction.BCC, (CpuAddressingMode.Relative, CpuOpcode.BCC));
        Register(CpuInstruction.BCS, (CpuAddressingMode.Relative, CpuOpcode.BCS));
        Register(CpuInstruction.BNE, (CpuAddressingMode.Relative, CpuOpcode.BNE));
        Register(CpuInstruction.BEQ, (CpuAddressingMode.Relative, CpuOpcode.BEQ));

        Register(CpuInstruction.BRK, (CpuAddressingMode.Implied, CpuOpcode.BRK));

        Register(CpuInstruction.CMP,
            (CpuAddressingMode.Immediate, CpuOpcode.CMP_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.CMP_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.CMP_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.CMP_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.CMP_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.CMP_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.CMP_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.CMP_INDY)
        );

        Register(CpuInstruction.CPX,
            (CpuAddressingMode.Immediate, CpuOpcode.CPX_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.CPX_ZP),
            (CpuAddressingMode.Absolute, CpuOpcode.CPX_ABS)
        );

        Register(CpuInstruction.CPY,
            (CpuAddressingMode.Immediate, CpuOpcode.CPY_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.CPY_ZP),
            (CpuAddressingMode.Absolute, CpuOpcode.CPY_ABS)
        );

        Register(CpuInstruction.DEC,
            (CpuAddressingMode.ZeroPage, CpuOpcode.DEC_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.DEC_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.DEC_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.DEC_ABSX)
        );

        Register(CpuInstruction.EOR,
            (CpuAddressingMode.Immediate, CpuOpcode.EOR_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.EOR_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.EOR_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.EOR_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.EOR_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.EOR_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.EOR_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.EOR_INDY)
        );

        Register(CpuInstruction.CLC, (CpuAddressingMode.Implied, CpuOpcode.CLC));
        Register(CpuInstruction.SEC, (CpuAddressingMode.Implied, CpuOpcode.SEC));
        Register(CpuInstruction.CLI, (CpuAddressingMode.Implied, CpuOpcode.CLI));
        Register(CpuInstruction.SEI, (CpuAddressingMode.Implied, CpuOpcode.SEI));
        Register(CpuInstruction.CLV, (CpuAddressingMode.Implied, CpuOpcode.CLV));
        Register(CpuInstruction.CLD, (CpuAddressingMode.Implied, CpuOpcode.CLD));
        Register(CpuInstruction.SED, (CpuAddressingMode.Implied, CpuOpcode.SED));

        Register(CpuInstruction.INC,
            (CpuAddressingMode.ZeroPage, CpuOpcode.INC_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.INC_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.INC_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.INC_ABSX)
        );

        Register(CpuInstruction.JMP,
            (CpuAddressingMode.Absolute, CpuOpcode.JMP_ABS),
            (CpuAddressingMode.Indirect, CpuOpcode.JMP_IND)
        );

        Register(CpuInstruction.JSR, (CpuAddressingMode.Absolute, CpuOpcode.JSR));

        Register(CpuInstruction.LDA,
            (CpuAddressingMode.Immediate, CpuOpcode.LDA_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.LDA_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.LDA_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.LDA_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.LDA_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.LDA_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.LDA_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.LDA_INDY)
        );

        Register(CpuInstruction.LDX,
            (CpuAddressingMode.Immediate, CpuOpcode.LDX_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.LDX_ZP),
            (CpuAddressingMode.ZeroPageY, CpuOpcode.LDX_ZPY),
            (CpuAddressingMode.Absolute, CpuOpcode.LDX_ABS),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.LDX_ABSY)
        );

        Register(CpuInstruction.LDY,
            (CpuAddressingMode.Immediate, CpuOpcode.LDY_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.LDY_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.LDY_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.LDY_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.LDY_ABSX)
        );

        Register(CpuInstruction.LSR,
            (CpuAddressingMode.Implied, CpuOpcode.LSR_ACC),
            (CpuAddressingMode.ZeroPage, CpuOpcode.LSR_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.LSR_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.LSR_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.LSR_ABSX)
        );

        Register(CpuInstruction.NOP, (CpuAddressingMode.Implied, CpuOpcode.NOP));

        Register(CpuInstruction.ORA,
            (CpuAddressingMode.Immediate, CpuOpcode.ORA_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.ORA_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.ORA_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.ORA_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.ORA_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.ORA_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.ORA_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.ORA_INDY)
        );

        Register(CpuInstruction.TAX, (CpuAddressingMode.Implied, CpuOpcode.TAX));
        Register(CpuInstruction.TXA, (CpuAddressingMode.Implied, CpuOpcode.TXA));
        Register(CpuInstruction.DEX, (CpuAddressingMode.Implied, CpuOpcode.DEX));
        Register(CpuInstruction.INX, (CpuAddressingMode.Implied, CpuOpcode.INX));
        Register(CpuInstruction.TAY, (CpuAddressingMode.Implied, CpuOpcode.TAY));
        Register(CpuInstruction.TYA, (CpuAddressingMode.Implied, CpuOpcode.TYA));
        Register(CpuInstruction.DEY, (CpuAddressingMode.Implied, CpuOpcode.DEY));
        Register(CpuInstruction.INY, (CpuAddressingMode.Implied, CpuOpcode.INY));

        Register(CpuInstruction.ROL,
            (CpuAddressingMode.Implied, CpuOpcode.ROL_ACC),
            (CpuAddressingMode.ZeroPage, CpuOpcode.ROL_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.ROL_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.ROL_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.ROL_ABSX)
        );

        Register(CpuInstruction.ROR,
            (CpuAddressingMode.Implied, CpuOpcode.ROR_ACC),
            (CpuAddressingMode.ZeroPage, CpuOpcode.ROR_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.ROR_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.ROR_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.ROR_ABSX)
        );

        Register(CpuInstruction.RTI, (CpuAddressingMode.Implied, CpuOpcode.RTI));

        Register(CpuInstruction.RTS, (CpuAddressingMode.Implied, CpuOpcode.RTS));

        Register(CpuInstruction.STA,
            (CpuAddressingMode.ZeroPage, CpuOpcode.STA_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.STA_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.STA_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.STA_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.STA_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.STA_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.STA_INDY)
        );

        Register(CpuInstruction.SBC,
            (CpuAddressingMode.Immediate, CpuOpcode.SBC_IM),
            (CpuAddressingMode.ZeroPage, CpuOpcode.SBC_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.SBC_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.SBC_ABS),
            (CpuAddressingMode.AbsoluteX, CpuOpcode.SBC_ABSX),
            (CpuAddressingMode.AbsoluteY, CpuOpcode.SBC_ABSY),
            (CpuAddressingMode.IndirectX, CpuOpcode.SBC_INDX),
            (CpuAddressingMode.IndirectY, CpuOpcode.SBC_INDY)
        );

        Register(CpuInstruction.TXS, (CpuAddressingMode.Implied, CpuOpcode.TXS));
        Register(CpuInstruction.TSX, (CpuAddressingMode.Implied, CpuOpcode.TSX));
        Register(CpuInstruction.PHA, (CpuAddressingMode.Implied, CpuOpcode.PHA));
        Register(CpuInstruction.PLA, (CpuAddressingMode.Implied, CpuOpcode.PLA));
        Register(CpuInstruction.PHP, (CpuAddressingMode.Implied, CpuOpcode.PHP));
        Register(CpuInstruction.PLP, (CpuAddressingMode.Implied, CpuOpcode.PLP));

        Register(CpuInstruction.STX,
            (CpuAddressingMode.ZeroPage, CpuOpcode.STX_ZP),
            (CpuAddressingMode.ZeroPageY, CpuOpcode.STX_ZPY),
            (CpuAddressingMode.Absolute, CpuOpcode.STX_ABS)
        );

        Register(CpuInstruction.STY,
            (CpuAddressingMode.ZeroPage, CpuOpcode.STY_ZP),
            (CpuAddressingMode.ZeroPageX, CpuOpcode.STY_ZPX),
            (CpuAddressingMode.Absolute, CpuOpcode.STY_ABS)
        );
    }

    public static bool TryEncodeOpcode(CpuInstruction op, CpuAddressingMode mode, out CpuOpcode opcode) =>
        toOpcode.TryGetValue((op, mode), out opcode);

    public static bool TryDecodeOpcode(CpuOpcode opcode, out (CpuInstruction, CpuAddressingMode) decode) =>
        fromOpcode.TryGetValue(opcode, out decode);

    private static void Register(CpuInstruction instr, params (CpuAddressingMode mode, CpuOpcode opcode)[] variants)
    {
        foreach (var (mode, opcode) in variants)
            Register(instr, mode, opcode);
    }

    private static void Register(CpuInstruction CpuInstruction, CpuAddressingMode mode, CpuOpcode opcode)
    {
        toOpcode[(CpuInstruction, mode)] = opcode;
        fromOpcode[opcode] = (CpuInstruction, mode);
    }
}

