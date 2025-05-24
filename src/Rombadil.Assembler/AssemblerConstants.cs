namespace Rombadil.Assembler;

internal class AssemblerConstants
{
    private readonly Dictionary<string, int> declarations = [];
    private readonly Dictionary<string, int> values = [];

    internal void Declare(string name, int statementIndex) =>
        declarations.Add(name, statementIndex);

    internal void SetValue(string name, int value) =>
        values.Add(name, value);

    internal bool TryGetStatementIndex(string name, out int statementIndex) =>
        declarations.TryGetValue(name, out statementIndex);

    internal bool TryGetValue(string name, out int value) =>
        values.TryGetValue(name, out value);
}
