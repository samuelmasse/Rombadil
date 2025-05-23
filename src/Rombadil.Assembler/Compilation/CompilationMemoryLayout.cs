namespace Rombadil.Assembler;

public class CompilationMemoryLayout(CompilationStatements statements)
{
    private readonly int?[] layout = new int?[statements.Statements.Length];

    public ref int? this[int statementIndex] => ref layout[statementIndex];
}
