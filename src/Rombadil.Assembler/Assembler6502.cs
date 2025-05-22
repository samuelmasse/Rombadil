namespace Rombadil.Assembler;

public class Assembler6502
{
    public byte[] Assemble(string[] source) => new CompilationUnit(source, new()).Compile();
}
