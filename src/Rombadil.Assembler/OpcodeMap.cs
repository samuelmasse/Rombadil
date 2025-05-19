namespace Rombadil.Assembler;

public class OpcodeMap
{
    private readonly Dictionary<(Instruction, AdressingMode), CpuOpcode> toOpcode = [];
    private readonly Dictionary<CpuOpcode, (Instruction, AdressingMode)> fromOpcode = [];

    public OpcodeMap()
    {
        Register(Instruction.ADC,
            (AdressingMode.Immediate, CpuOpcode.ADC_IM),
            (AdressingMode.ZeroPage, CpuOpcode.ADC_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.ADC_ZPX),
            (AdressingMode.Absolute, CpuOpcode.ADC_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.ADC_ABSX),
            (AdressingMode.AbsoluteY, CpuOpcode.ADC_ABSY),
            (AdressingMode.IndirectX, CpuOpcode.ADC_INDX),
            (AdressingMode.IndirectY, CpuOpcode.ADC_INDY)
        );

        Register(Instruction.AND,
            (AdressingMode.Immediate, CpuOpcode.AND_IM),
            (AdressingMode.ZeroPage, CpuOpcode.AND_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.AND_ZPX),
            (AdressingMode.Absolute, CpuOpcode.AND_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.AND_ABSX),
            (AdressingMode.AbsoluteY, CpuOpcode.AND_ABSY),
            (AdressingMode.IndirectX, CpuOpcode.AND_INDX),
            (AdressingMode.IndirectY, CpuOpcode.AND_INDY)
        );

        Register(Instruction.ASL,
            (AdressingMode.Implied, CpuOpcode.ASL_ACC),
            (AdressingMode.ZeroPage, CpuOpcode.ASL_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.ASL_ZPX),
            (AdressingMode.Absolute, CpuOpcode.ASL_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.ASL_ABSX)
        );

        Register(Instruction.BPL, (AdressingMode.Relative, CpuOpcode.BPL));
        Register(Instruction.BMI, (AdressingMode.Relative, CpuOpcode.BMI));
        Register(Instruction.BVC, (AdressingMode.Relative, CpuOpcode.BVC));
        Register(Instruction.BVS, (AdressingMode.Relative, CpuOpcode.BVS));
        Register(Instruction.BCC, (AdressingMode.Relative, CpuOpcode.BCC));
        Register(Instruction.BCS, (AdressingMode.Relative, CpuOpcode.BCS));
        Register(Instruction.BNE, (AdressingMode.Relative, CpuOpcode.BNE));
        Register(Instruction.BEQ, (AdressingMode.Relative, CpuOpcode.BEQ));

        Register(Instruction.BRK, (AdressingMode.Implied, CpuOpcode.BRK));

        Register(Instruction.CMP,
            (AdressingMode.Immediate, CpuOpcode.CMP_IM),
            (AdressingMode.ZeroPage, CpuOpcode.CMP_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.CMP_ZPX),
            (AdressingMode.Absolute, CpuOpcode.CMP_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.CMP_ABSX),
            (AdressingMode.AbsoluteY, CpuOpcode.CMP_ABSY),
            (AdressingMode.IndirectX, CpuOpcode.CMP_INDX),
            (AdressingMode.IndirectY, CpuOpcode.CMP_INDY)
        );

        Register(Instruction.CPX,
            (AdressingMode.Immediate, CpuOpcode.CPX_IM),
            (AdressingMode.ZeroPage, CpuOpcode.CPX_ZP),
            (AdressingMode.Absolute, CpuOpcode.CPX_ABS)
        );

        Register(Instruction.CPY,
            (AdressingMode.Immediate, CpuOpcode.CPY_IM),
            (AdressingMode.ZeroPage, CpuOpcode.CPY_ZP),
            (AdressingMode.Absolute, CpuOpcode.CPY_ABS)
        );

        Register(Instruction.DEC,
            (AdressingMode.ZeroPage, CpuOpcode.DEC_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.DEC_ZPX),
            (AdressingMode.Absolute, CpuOpcode.DEC_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.DEC_ABSX)
        );

        Register(Instruction.EOR,
            (AdressingMode.Immediate, CpuOpcode.EOR_IM),
            (AdressingMode.ZeroPage, CpuOpcode.EOR_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.EOR_ZPX),
            (AdressingMode.Absolute, CpuOpcode.EOR_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.EOR_ABSX),
            (AdressingMode.AbsoluteY, CpuOpcode.EOR_ABSY),
            (AdressingMode.IndirectX, CpuOpcode.EOR_INDX),
            (AdressingMode.IndirectY, CpuOpcode.EOR_INDY)
        );

        Register(Instruction.CLC, (AdressingMode.Implied, CpuOpcode.CLC));
        Register(Instruction.SEC, (AdressingMode.Implied, CpuOpcode.SEC));
        Register(Instruction.CLI, (AdressingMode.Implied, CpuOpcode.CLI));
        Register(Instruction.SEI, (AdressingMode.Implied, CpuOpcode.SEI));
        Register(Instruction.CLV, (AdressingMode.Implied, CpuOpcode.CLV));
        Register(Instruction.CLD, (AdressingMode.Implied, CpuOpcode.CLD));
        Register(Instruction.SED, (AdressingMode.Implied, CpuOpcode.SED));

        Register(Instruction.INC,
            (AdressingMode.ZeroPage, CpuOpcode.INC_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.INC_ZPX),
            (AdressingMode.Absolute, CpuOpcode.INC_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.INC_ABSX)
        );

        Register(Instruction.JMP,
            (AdressingMode.Absolute, CpuOpcode.JMP_ABS),
            (AdressingMode.Indirect, CpuOpcode.JMP_IND)
        );

        Register(Instruction.JSR, (AdressingMode.Absolute, CpuOpcode.JSR));

        Register(Instruction.LDA,
            (AdressingMode.Immediate, CpuOpcode.LDA_IM),
            (AdressingMode.ZeroPage, CpuOpcode.LDA_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.LDA_ZPX),
            (AdressingMode.Absolute, CpuOpcode.LDA_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.LDA_ABSX),
            (AdressingMode.AbsoluteY, CpuOpcode.LDA_ABSY),
            (AdressingMode.IndirectX, CpuOpcode.LDA_INDX),
            (AdressingMode.IndirectY, CpuOpcode.LDA_INDY)
        );

        Register(Instruction.LDX,
            (AdressingMode.Immediate, CpuOpcode.LDX_IM),
            (AdressingMode.ZeroPage, CpuOpcode.LDX_ZP),
            (AdressingMode.ZeroPageY, CpuOpcode.LDX_ZPY),
            (AdressingMode.Absolute, CpuOpcode.LDX_ABS),
            (AdressingMode.AbsoluteY, CpuOpcode.LDX_ABSY)
        );

        Register(Instruction.LDY,
            (AdressingMode.Immediate, CpuOpcode.LDY_IM),
            (AdressingMode.ZeroPage, CpuOpcode.LDY_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.LDY_ZPX),
            (AdressingMode.Absolute, CpuOpcode.LDY_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.LDY_ABSX)
        );

        Register(Instruction.LSR,
            (AdressingMode.Implied, CpuOpcode.LSR_ACC),
            (AdressingMode.ZeroPage, CpuOpcode.LSR_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.LSR_ZPX),
            (AdressingMode.Absolute, CpuOpcode.LSR_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.LSR_ABSX)
        );

        Register(Instruction.NOP, (AdressingMode.Implied, CpuOpcode.NOP));

        Register(Instruction.ORA,
            (AdressingMode.Immediate, CpuOpcode.ORA_IM),
            (AdressingMode.ZeroPage, CpuOpcode.ORA_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.ORA_ZPX),
            (AdressingMode.Absolute, CpuOpcode.ORA_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.ORA_ABSX),
            (AdressingMode.AbsoluteY, CpuOpcode.ORA_ABSY),
            (AdressingMode.IndirectX, CpuOpcode.ORA_INDX),
            (AdressingMode.IndirectY, CpuOpcode.ORA_INDY)
        );

        Register(Instruction.TAX, (AdressingMode.Implied, CpuOpcode.TAX));
        Register(Instruction.TXA, (AdressingMode.Implied, CpuOpcode.TXA));
        Register(Instruction.DEX, (AdressingMode.Implied, CpuOpcode.DEX));
        Register(Instruction.INX, (AdressingMode.Implied, CpuOpcode.INX));
        Register(Instruction.TAY, (AdressingMode.Implied, CpuOpcode.TAY));
        Register(Instruction.TYA, (AdressingMode.Implied, CpuOpcode.TYA));
        Register(Instruction.DEY, (AdressingMode.Implied, CpuOpcode.DEY));
        Register(Instruction.INY, (AdressingMode.Implied, CpuOpcode.INY));

        Register(Instruction.ROL,
            (AdressingMode.Implied, CpuOpcode.ROL_ACC),
            (AdressingMode.ZeroPage, CpuOpcode.ROL_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.ROL_ZPX),
            (AdressingMode.Absolute, CpuOpcode.ROL_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.ROL_ABSX)
        );

        Register(Instruction.ROR,
            (AdressingMode.Implied, CpuOpcode.ROR_ACC),
            (AdressingMode.ZeroPage, CpuOpcode.ROR_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.ROR_ZPX),
            (AdressingMode.Absolute, CpuOpcode.ROR_ABS),
            (AdressingMode.AbsoluteX, CpuOpcode.ROR_ABSX)
        );

        Register(Instruction.RTI, (AdressingMode.Implied, CpuOpcode.RTI));

        Register(Instruction.TXS, (AdressingMode.Implied, CpuOpcode.TXS));
        Register(Instruction.TSX, (AdressingMode.Implied, CpuOpcode.TSX));
        Register(Instruction.PHA, (AdressingMode.Implied, CpuOpcode.PHA));
        Register(Instruction.PLA, (AdressingMode.Implied, CpuOpcode.PLA));
        Register(Instruction.PHP, (AdressingMode.Implied, CpuOpcode.PHP));
        Register(Instruction.PLP, (AdressingMode.Implied, CpuOpcode.PLP));

        Register(Instruction.STX,
            (AdressingMode.ZeroPage, CpuOpcode.STX_ZP),
            (AdressingMode.ZeroPageY, CpuOpcode.STX_ZPY),
            (AdressingMode.Absolute, CpuOpcode.STX_ABS)
        );

        Register(Instruction.STY,
            (AdressingMode.ZeroPage, CpuOpcode.STY_ZP),
            (AdressingMode.ZeroPageX, CpuOpcode.STY_ZPX),
            (AdressingMode.Absolute, CpuOpcode.STY_ABS)
        );
    }

    public CpuOpcode EncodeOpcode(Instruction op, AdressingMode mode)
    {
        if (toOpcode.TryGetValue((op, mode), out var opcode))
            return opcode;
        else throw new InvalidOperationException($"No opcode found for {op} with {mode} addressing");
    }

    public (Instruction, AdressingMode) DecodeOpcode(CpuOpcode opcode)
    {
        if (fromOpcode.TryGetValue(opcode, out var pair))
            return pair;
        else throw new InvalidOperationException($"Opcode {opcode} is not registered in the mapping");
    }

    private void Register(Instruction instr, params (AdressingMode mode, CpuOpcode opcode)[] variants)
    {
        foreach (var (mode, opcode) in variants)
            Register(instr, mode, opcode);
    }

    private void Register(Instruction instruction, AdressingMode mode, CpuOpcode opcode)
    {
        toOpcode[(instruction, mode)] = opcode;
        fromOpcode[opcode] = (instruction, mode);
    }
}

