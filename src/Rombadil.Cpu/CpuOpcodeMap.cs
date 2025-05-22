namespace Rombadil.Cpu;

public static class CpuOpcodeMap
{
    private static readonly Dictionary<(CpuInstruction, CpuAdressingMode), CpuOpcode> toOpcode = [];
    private static readonly Dictionary<CpuOpcode, (CpuInstruction, CpuAdressingMode)> fromOpcode = [];

    static CpuOpcodeMap()
    {
        Register(CpuInstruction.ADC,
            (CpuAdressingMode.Immediate, CpuOpcode.ADC_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.ADC_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.ADC_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.ADC_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.ADC_ABSX),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.ADC_ABSY),
            (CpuAdressingMode.IndirectX, CpuOpcode.ADC_INDX),
            (CpuAdressingMode.IndirectY, CpuOpcode.ADC_INDY)
        );

        Register(CpuInstruction.AND,
            (CpuAdressingMode.Immediate, CpuOpcode.AND_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.AND_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.AND_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.AND_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.AND_ABSX),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.AND_ABSY),
            (CpuAdressingMode.IndirectX, CpuOpcode.AND_INDX),
            (CpuAdressingMode.IndirectY, CpuOpcode.AND_INDY)
        );

        Register(CpuInstruction.ASL,
            (CpuAdressingMode.Implied, CpuOpcode.ASL_ACC),
            (CpuAdressingMode.ZeroPage, CpuOpcode.ASL_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.ASL_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.ASL_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.ASL_ABSX)
        );

        Register(CpuInstruction.BIT,
            (CpuAdressingMode.ZeroPage, CpuOpcode.BIT_ZP),
            (CpuAdressingMode.Absolute, CpuOpcode.BIT_ABS)
        );

        Register(CpuInstruction.BPL, (CpuAdressingMode.Relative, CpuOpcode.BPL));
        Register(CpuInstruction.BMI, (CpuAdressingMode.Relative, CpuOpcode.BMI));
        Register(CpuInstruction.BVC, (CpuAdressingMode.Relative, CpuOpcode.BVC));
        Register(CpuInstruction.BVS, (CpuAdressingMode.Relative, CpuOpcode.BVS));
        Register(CpuInstruction.BCC, (CpuAdressingMode.Relative, CpuOpcode.BCC));
        Register(CpuInstruction.BCS, (CpuAdressingMode.Relative, CpuOpcode.BCS));
        Register(CpuInstruction.BNE, (CpuAdressingMode.Relative, CpuOpcode.BNE));
        Register(CpuInstruction.BEQ, (CpuAdressingMode.Relative, CpuOpcode.BEQ));

        Register(CpuInstruction.BRK, (CpuAdressingMode.Implied, CpuOpcode.BRK));

        Register(CpuInstruction.CMP,
            (CpuAdressingMode.Immediate, CpuOpcode.CMP_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.CMP_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.CMP_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.CMP_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.CMP_ABSX),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.CMP_ABSY),
            (CpuAdressingMode.IndirectX, CpuOpcode.CMP_INDX),
            (CpuAdressingMode.IndirectY, CpuOpcode.CMP_INDY)
        );

        Register(CpuInstruction.CPX,
            (CpuAdressingMode.Immediate, CpuOpcode.CPX_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.CPX_ZP),
            (CpuAdressingMode.Absolute, CpuOpcode.CPX_ABS)
        );

        Register(CpuInstruction.CPY,
            (CpuAdressingMode.Immediate, CpuOpcode.CPY_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.CPY_ZP),
            (CpuAdressingMode.Absolute, CpuOpcode.CPY_ABS)
        );

        Register(CpuInstruction.DEC,
            (CpuAdressingMode.ZeroPage, CpuOpcode.DEC_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.DEC_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.DEC_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.DEC_ABSX)
        );

        Register(CpuInstruction.EOR,
            (CpuAdressingMode.Immediate, CpuOpcode.EOR_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.EOR_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.EOR_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.EOR_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.EOR_ABSX),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.EOR_ABSY),
            (CpuAdressingMode.IndirectX, CpuOpcode.EOR_INDX),
            (CpuAdressingMode.IndirectY, CpuOpcode.EOR_INDY)
        );

        Register(CpuInstruction.CLC, (CpuAdressingMode.Implied, CpuOpcode.CLC));
        Register(CpuInstruction.SEC, (CpuAdressingMode.Implied, CpuOpcode.SEC));
        Register(CpuInstruction.CLI, (CpuAdressingMode.Implied, CpuOpcode.CLI));
        Register(CpuInstruction.SEI, (CpuAdressingMode.Implied, CpuOpcode.SEI));
        Register(CpuInstruction.CLV, (CpuAdressingMode.Implied, CpuOpcode.CLV));
        Register(CpuInstruction.CLD, (CpuAdressingMode.Implied, CpuOpcode.CLD));
        Register(CpuInstruction.SED, (CpuAdressingMode.Implied, CpuOpcode.SED));

        Register(CpuInstruction.INC,
            (CpuAdressingMode.ZeroPage, CpuOpcode.INC_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.INC_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.INC_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.INC_ABSX)
        );

        Register(CpuInstruction.JMP,
            (CpuAdressingMode.Absolute, CpuOpcode.JMP_ABS),
            (CpuAdressingMode.Indirect, CpuOpcode.JMP_IND)
        );

        Register(CpuInstruction.JSR, (CpuAdressingMode.Absolute, CpuOpcode.JSR));

        Register(CpuInstruction.LDA,
            (CpuAdressingMode.Immediate, CpuOpcode.LDA_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.LDA_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.LDA_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.LDA_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.LDA_ABSX),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.LDA_ABSY),
            (CpuAdressingMode.IndirectX, CpuOpcode.LDA_INDX),
            (CpuAdressingMode.IndirectY, CpuOpcode.LDA_INDY)
        );

        Register(CpuInstruction.LDX,
            (CpuAdressingMode.Immediate, CpuOpcode.LDX_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.LDX_ZP),
            (CpuAdressingMode.ZeroPageY, CpuOpcode.LDX_ZPY),
            (CpuAdressingMode.Absolute, CpuOpcode.LDX_ABS),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.LDX_ABSY)
        );

        Register(CpuInstruction.LDY,
            (CpuAdressingMode.Immediate, CpuOpcode.LDY_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.LDY_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.LDY_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.LDY_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.LDY_ABSX)
        );

        Register(CpuInstruction.LSR,
            (CpuAdressingMode.Implied, CpuOpcode.LSR_ACC),
            (CpuAdressingMode.ZeroPage, CpuOpcode.LSR_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.LSR_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.LSR_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.LSR_ABSX)
        );

        Register(CpuInstruction.NOP, (CpuAdressingMode.Implied, CpuOpcode.NOP));

        Register(CpuInstruction.ORA,
            (CpuAdressingMode.Immediate, CpuOpcode.ORA_IM),
            (CpuAdressingMode.ZeroPage, CpuOpcode.ORA_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.ORA_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.ORA_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.ORA_ABSX),
            (CpuAdressingMode.AbsoluteY, CpuOpcode.ORA_ABSY),
            (CpuAdressingMode.IndirectX, CpuOpcode.ORA_INDX),
            (CpuAdressingMode.IndirectY, CpuOpcode.ORA_INDY)
        );

        Register(CpuInstruction.TAX, (CpuAdressingMode.Implied, CpuOpcode.TAX));
        Register(CpuInstruction.TXA, (CpuAdressingMode.Implied, CpuOpcode.TXA));
        Register(CpuInstruction.DEX, (CpuAdressingMode.Implied, CpuOpcode.DEX));
        Register(CpuInstruction.INX, (CpuAdressingMode.Implied, CpuOpcode.INX));
        Register(CpuInstruction.TAY, (CpuAdressingMode.Implied, CpuOpcode.TAY));
        Register(CpuInstruction.TYA, (CpuAdressingMode.Implied, CpuOpcode.TYA));
        Register(CpuInstruction.DEY, (CpuAdressingMode.Implied, CpuOpcode.DEY));
        Register(CpuInstruction.INY, (CpuAdressingMode.Implied, CpuOpcode.INY));

        Register(CpuInstruction.ROL,
            (CpuAdressingMode.Implied, CpuOpcode.ROL_ACC),
            (CpuAdressingMode.ZeroPage, CpuOpcode.ROL_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.ROL_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.ROL_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.ROL_ABSX)
        );

        Register(CpuInstruction.ROR,
            (CpuAdressingMode.Implied, CpuOpcode.ROR_ACC),
            (CpuAdressingMode.ZeroPage, CpuOpcode.ROR_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.ROR_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.ROR_ABS),
            (CpuAdressingMode.AbsoluteX, CpuOpcode.ROR_ABSX)
        );

        Register(CpuInstruction.RTI, (CpuAdressingMode.Implied, CpuOpcode.RTI));

        Register(CpuInstruction.TXS, (CpuAdressingMode.Implied, CpuOpcode.TXS));
        Register(CpuInstruction.TSX, (CpuAdressingMode.Implied, CpuOpcode.TSX));
        Register(CpuInstruction.PHA, (CpuAdressingMode.Implied, CpuOpcode.PHA));
        Register(CpuInstruction.PLA, (CpuAdressingMode.Implied, CpuOpcode.PLA));
        Register(CpuInstruction.PHP, (CpuAdressingMode.Implied, CpuOpcode.PHP));
        Register(CpuInstruction.PLP, (CpuAdressingMode.Implied, CpuOpcode.PLP));

        Register(CpuInstruction.STX,
            (CpuAdressingMode.ZeroPage, CpuOpcode.STX_ZP),
            (CpuAdressingMode.ZeroPageY, CpuOpcode.STX_ZPY),
            (CpuAdressingMode.Absolute, CpuOpcode.STX_ABS)
        );

        Register(CpuInstruction.STY,
            (CpuAdressingMode.ZeroPage, CpuOpcode.STY_ZP),
            (CpuAdressingMode.ZeroPageX, CpuOpcode.STY_ZPX),
            (CpuAdressingMode.Absolute, CpuOpcode.STY_ABS)
        );
    }

    public static CpuOpcode EncodeOpcode(CpuInstruction op, CpuAdressingMode mode)
    {
        if (toOpcode.TryGetValue((op, mode), out var opcode))
            return opcode;
        else throw new InvalidOperationException($"No opcode found for {op} with {mode} addressing");
    }

    public static (CpuInstruction, CpuAdressingMode) DecodeOpcode(CpuOpcode opcode)
    {
        if (fromOpcode.TryGetValue(opcode, out var pair))
            return pair;
        else throw new InvalidOperationException($"Opcode {opcode} is not registered in the mapping");
    }

    private static void Register(CpuInstruction instr, params (CpuAdressingMode mode, CpuOpcode opcode)[] variants)
    {
        foreach (var (mode, opcode) in variants)
            Register(instr, mode, opcode);
    }

    private static void Register(CpuInstruction CpuInstruction, CpuAdressingMode mode, CpuOpcode opcode)
    {
        toOpcode[(CpuInstruction, mode)] = opcode;
        fromOpcode[opcode] = (CpuInstruction, mode);
    }
}

