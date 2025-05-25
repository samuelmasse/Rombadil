namespace Rombadil.Assembler;

public class Assembler6502(IReadOnlyList<AssemblerSegment> segments, IFileSystem fileSystem)
{
    public Assembler6502(IReadOnlyList<AssemblerSegment> segments) : this(segments, new FileSystem()) { }
    public Assembler6502() : this([], new FileSystem()) { }

    public byte[] Assemble(string[] lines)
    {
        var statements = new List<AssemblerStatement>();
        var declarations = new Dictionary<string, int>();
        var values = new Dictionary<string, (int, bool)>();
        var output = new List<byte>();

        var parser = new AssemblerParser(statements);
        var resolver = new AssemblerResolver(statements, declarations, values);
        var addresser = new AssemblerAddresser(resolver);
        var emitter = new AssemblerEmitter(segments, statements, resolver, output);

        new AssemblerExecution(
            fileSystem,
            segments,
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
