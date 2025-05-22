namespace Rombadil.Assembler;

public class CompilationSource(string[] lines)
{
    public ReadOnlyMemory<string> Lines => lines;
}
