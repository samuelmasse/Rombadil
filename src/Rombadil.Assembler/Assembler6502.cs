namespace Rombadil.Assembler;

public class Assembler6502
{
    public byte[] Assemble(string[] lines)
    {
        var statementParser = new StatementParser();
        var statements = new List<Statement>();
        var constants = new CompilationConstants();
        var resolver = new CompilationResolver(statements, constants);
        var addressingModeResolver = new CompilationAddressingModeResolver(resolver);

        return new CompilationStage(
            lines,
            statementParser,
            statements,
            constants,
            resolver,
            addressingModeResolver).Compile();
    }
}
