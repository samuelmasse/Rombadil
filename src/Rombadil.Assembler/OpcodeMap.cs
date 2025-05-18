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

