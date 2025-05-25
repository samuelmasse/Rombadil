namespace Rombadil.Assembler;

public class Assembler6502Exception(int line, string error) : Exception($"{line}: {error}")
{
    public int Line => line;
    public string Error => error;
}
