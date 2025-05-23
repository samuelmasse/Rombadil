namespace Rombadil.Assembler;

public class CompilationInstructionStatements(CompilationStatements statements)
{
    private readonly InstructionStatement?[] statements = new InstructionStatement?[statements.Statements.Length];

    public ref InstructionStatement? this[int statementIndex] => ref statements[statementIndex];
}
