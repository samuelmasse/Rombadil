namespace Rombadil.Assembler;

public class Assembler6502
{
    public byte[] Assemble(string[] lines)
    {
        var statements = new List<AssemblerStatement>();
        var parser = new AssemblerParser(statements);
        var constants = new AssemblerConstants();
        var resolver = new AssemblerResolver(statements, constants);
        var addresser = new AssemblerAddresser(resolver);
        var output = new List<byte>();
        var emitter = new AssemblerEmitter(statements, resolver, output);

        new AssemblerExecution(
            lines,
            statements,
            parser,
            constants,
            resolver,
            addresser,
            emitter).Compile();

        return [.. output];
    }
}
