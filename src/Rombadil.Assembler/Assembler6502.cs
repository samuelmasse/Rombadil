namespace Rombadil.Assembler;

public class Assembler6502
{
    public byte[] Assemble(string[] lines) => new CompilationStage1(new(lines), new()).Compile();
}
