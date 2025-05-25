namespace Rombadil.Assembler;

public class Assembler6502Exception(int line, string error) : Exception($"{line + 1}: {error}")
{
    public int Line => line;
    public string Error => error;
}
