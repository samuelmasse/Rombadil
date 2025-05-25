namespace Rombadil.Assembler;

internal record class AssemblerStatement(int LineNumber, string Name, string Value, AssemblerStatementType Type)
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

internal record class AssemblerDirective(AssemblerDirectiveType Type, string[] Expressions);

internal enum AssemblerDirectiveType
{
    Segment, // TODO: remove this
    Org,
    Byte,
    Word,
    Incbin
}

internal record class AssemblerInstruction(CpuInstruction Instruction, CpuAddressingMode AddressingMode, string Expression);
