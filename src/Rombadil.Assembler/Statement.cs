namespace Rombadil.Assembler;

public record class Statement(string Name, string Value, StatementType Type)
{
    public DirectiveStatement? DirectiveStatement;
    public InstructionStatement? InstructionStatement;
    public int? MemoryLocation;
}

public enum StatementType
{
    Constant,
    Label,
    Operation
}

public record struct DirectiveStatement(DirectiveType Type, string[] Expressions);

public enum DirectiveType
{
    Segment, // TODO: remove this
    Org,
    Byte,
    Word,
    Incbin
}

public record struct InstructionStatement(CpuInstruction Instruction, CpuAddressingMode AddressingMode, string Expression);
