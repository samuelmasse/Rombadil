namespace Rombadil.Assembler;

public class CompilationStage1(CompilationSource source, StatementParser statementParser)
{
    public byte[] Compile()
    {
        var statements = new CompilationStatements(statementParser.Parse(source.Lines));
        var constants = new CompilationConstants();
        var equationParser = new EquationParser();
        var numberParser = new NumberParser();
        var resolver = new CompilationResolver(statements, constants, equationParser, numberParser);

        return new CompilationStage2(statements, constants, resolver).Compile();
    }
}
