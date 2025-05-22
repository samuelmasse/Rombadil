namespace Rombadil.Assembler;

public class CompilationConstants
{
    private readonly Dictionary<string, int> declarations = [];
    private readonly Dictionary<string, int> values = [];

    public void Declare(string name, int statementIndex) =>
        declarations.Add(name, statementIndex);

    public void SetValue(string name, int value) =>
        values.Add(name, value);

    public bool TryGetStatementIndex(string name, out int statementIndex) =>
        declarations.TryGetValue(name, out statementIndex);

    public bool TryGetValue(string name, out int value) =>
        values.TryGetValue(name, out value);
}
