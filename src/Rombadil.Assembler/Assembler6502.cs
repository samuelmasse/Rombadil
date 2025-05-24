namespace Rombadil.Assembler;

public class Assembler6502
{
    public byte[] Assemble(string[] lines)
    {
        var statements = new List<AssemblerStatement>();
        var declarations = new Dictionary<string, int>();
        var values = new Dictionary<string, int>();
        var output = new List<byte>();

        var parser = new AssemblerParser(statements);
        var resolver = new AssemblerResolver(statements, declarations, values);
        var addresser = new AssemblerAddresser(resolver);
        var emitter = new AssemblerEmitter(statements, resolver, output);

        new AssemblerExecution(
            lines,
            statements,
            declarations,
            values,
            parser,
            resolver,
            addresser,
            emitter).Compile();

        return [.. output];
    }
}
