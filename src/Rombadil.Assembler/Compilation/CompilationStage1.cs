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
        var adressingModeResolver = new CompilationAdressingModeResolver(resolver);
        var instructionStatements = new CompilationInstructionStatements(statements);
        var directiveStatements = new CompilationDirectiveStatements(statements);
        var memoryLayout = new CompilationMemoryLayout(statements);

        return new CompilationStage2(
            statements,
            constants,
            resolver,
            adressingModeResolver,
            instructionStatements,
            directiveStatements,
            memoryLayout).Compile();
    }
}
