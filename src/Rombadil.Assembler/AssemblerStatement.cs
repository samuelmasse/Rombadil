namespace Rombadil.Assembler;

internal record class AssemblerStatement(string Name, string Value, AssemblerStatementType Type)
{
    internal AssemblerDirective? Directive;
    internal AssemblerInstruction? Instruction;
    internal int? MemoryLocation;
}

internal enum AssemblerStatementType
{
    Constant,
    Label,
    Operation
}

internal record struct AssemblerDirective(AssemblerDirectiveType Type, string[] Expressions);

internal enum AssemblerDirectiveType
{
    Segment, // TODO: remove this
    Org,
    Byte,
    Word,
    Incbin
}

internal record struct AssemblerInstruction(CpuInstruction Instruction, CpuAddressingMode AddressingMode, string Expression);
