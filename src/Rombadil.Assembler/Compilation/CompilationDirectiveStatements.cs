namespace Rombadil.Assembler;

public class CompilationDirectiveStatements(CompilationStatements statements)
{
    private readonly DirectiveStatement?[] statements = new DirectiveStatement?[statements.Statements.Length];

    public ref DirectiveStatement? this[int statementIndex] => ref statements[statementIndex];
}
